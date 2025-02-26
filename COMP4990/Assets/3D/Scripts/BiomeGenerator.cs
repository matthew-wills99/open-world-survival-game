using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

using static Utils;
using static DistanceFunctions;

        /*
        Biomes will be generated such that:
        
        *Plains will be the default biome

        1. Different biomes will be generated using different distance functions:
            - Manhattan distance
            - Euclidean distance
            - Minkowski distance
            This will help to create unique shapes of different biomes.
        
        2. Each biome will be generated within a different radius from the centre chunks:
            - Snow biomes will be found further from the centre
            - Forest biomes will be found closer to the centre
            This will allow us to maintain some standard of difficulty for each biome, biomes further from the centre can contain
            more difficult challenges (enemies, environment, bosses), and late-game resources than biomes closer to the centre.

            Radius of biomes:
                - Close: Plains
                - Medium: Forest, Wasteland
                - Far: Desert, Snow
        
        3. Each biome will have an opposite biome:
            - Snow biomes will be opposite desert biomes (in the quadrant opposite both axis)

            Opposite biomes:
                - Snow and Desert
                - Forest and Wasteland

        4. Each biome will influence the placement of environment objects (trees, rocks, ores, etc..) within them differently
        */

        /*
        Initialize biome map to plains only

        generate pairs of biomes:
            generate snow and desert
            generate forest and wasteland

            maybe these things should be generated before water
        */

[CreateAssetMenu(fileName = "BiomeGenerator", menuName = "Biome Generator")]
public class BiomeGenerator : ScriptableObject
{
    [SerializeField] private List<Biome> biomes; // should be the default biome by default
    [SerializeField] private Biome defaultBiome;

    private int chunkSize;

    private Vector2Int forestWorldOrigin;
    private Vector2Int desertWorldOrigin;
    private Vector2Int wastelandWorldOrigin;
    private Vector2Int snowWorldOrigin;

    private Dictionary<EBiome, Vector2Int> biomeOrigins = new Dictionary<EBiome, Vector2Int>();

    public Dictionary<long, Biome[,]> GenerateBiomeMap(System.Random random)
    {
        chunkSize = GameManager.Instance.chunkSize;

        Dictionary<long, Biome[,]> biomeMap = new Dictionary<long, Biome[,]>();

        InitializeBiomeMap(biomeMap);

        PlaceBiomes(random);

        foreach(Biome biome in biomes)
        {
            SpreadBiome(biome, biomeOrigins[biome.biome], biome.biomeRadius, biomeMap, random);
        }

        return biomeMap;
    }

    private void InitializeBiomeMap(Dictionary<long, Biome[,]> biomeMap)
    {
        int halfMap = GameManager.Instance.mapGenerator.halfMap;

        for(int cx = -halfMap; cx < halfMap; cx++)
        {
            for(int cy = -halfMap; cy < halfMap; cy++)
            {
                biomeMap.Add(GetChunkKey(cx, cy), InitializeBiomeArray(chunkSize));
            }
        }   
    }

    private Biome[,] InitializeBiomeArray(int chunkSize)
    {
        Biome[,] biomeArray = new Biome[chunkSize, chunkSize];
        for(int x = 0; x < chunkSize; x++)
        {
            for(int y = 0; y < chunkSize; y++)
            {
                biomeArray[x, y] = defaultBiome;
            }
        }
        return biomeArray;
    }

    private void PlaceBiomes(System.Random random)
    {
        // will hardcode some values because fuck it
        // wasteland and snow must be minimum of quartermap to halfmap away from 0, 0
        // forest and desert must be 4 chunks to quartermap away from 0, 0
        int halfMap = GameManager.Instance.mapGenerator.halfMap;
        int quarterMap = halfMap / 2;
        int closeBiomeMinimum = 4;
        //idgaf about no water low key
        // pick close biomes first 
        // forest close (left) 1
        // desert close (down) 2
        // wasteland far (right) 1
        // snow far (up) 2
        // pick by the chunk, start in the centre of a chunk

        Vector2Int forestOrigin = new Vector2Int
        (
            random.Next(-quarterMap, closeBiomeMinimum),
            random.Next(-quarterMap, quarterMap)
        );

        Vector2Int wastelandOrigin = new Vector2Int
        (
            random.Next(closeBiomeMinimum, quarterMap),
            random.Next(-halfMap, halfMap)
        );

        Vector2Int snowOrigin = new Vector2Int
        (
            random.Next(-halfMap, halfMap),
            random.Next(quarterMap, halfMap)
        );

        Vector2Int desertOrigin = new Vector2Int
        (
            random.Next(-quarterMap, quarterMap),
            random.Next(-halfMap, -quarterMap)
        );

        forestWorldOrigin = ChunkToWorldPos(forestOrigin.x, forestOrigin.y, chunkSize / 2, chunkSize / 2, chunkSize);
        desertWorldOrigin = ChunkToWorldPos(desertOrigin.x, desertOrigin.y, chunkSize / 2, chunkSize / 2, chunkSize);
        wastelandWorldOrigin = ChunkToWorldPos(wastelandOrigin.x, wastelandOrigin.y, chunkSize / 2, chunkSize / 2, chunkSize);
        snowWorldOrigin = ChunkToWorldPos(snowOrigin.x, snowOrigin.y, chunkSize / 2, chunkSize / 2, chunkSize);

        biomeOrigins.Add(EBiome.Forest, forestWorldOrigin);
        biomeOrigins.Add(EBiome.Desert, desertWorldOrigin);
        biomeOrigins.Add(EBiome.Wasteland, wastelandWorldOrigin);
        biomeOrigins.Add(EBiome.Snow, snowWorldOrigin);
    }

    private void SpreadBiome(Biome biome, Vector2Int origin, int radius, Dictionary<long, Biome[,]> biomeMap, System.Random random)
    {
        int randomDistanceFunctionID = random.Next(1, 3+1); // random distance function picker

        for(int dx = -radius; dx < radius; dx++)
        {
            for(int dy = -radius; dy < radius; dy++)
            {
                Vector2Int pos = origin + new Vector2Int(dx, dy);
                (int cx, int cy, int bx, int by) = WorldToChunkPos(pos.x, pos.y, chunkSize);

                // biomes will use random distance functions each time because i said so
                float distance = UseDistanceFunctionById(new Vector2Int(dx, dy), randomDistanceFunctionID); // random distance function user
                int randomOffset = random.Next(-2, 2+1); // random offset so the biomes are not generated perfectly uniform along the edges
                if(distance <= (radius + randomOffset))
                {
                    if(biomeMap.ContainsKey(GetChunkKey(cx, cy)))
                    {
                        biomeMap[GetChunkKey(cx, cy)][bx, by] = biome;
                    }
                }
            }
        }
    }

    private int GetQuadrant(Vector2Int position)
    {
        if (position.x < 0 && position.y > 0) return 1; // Top-left
        if (position.x > 0 && position.y > 0) return 2; // Top-right
        if (position.x < 0 && position.y < 0) return 3; // Bottom-left
        if (position.x > 0 && position.y < 0) return 4; // Bottom-right
        return 2; // default is 2 because 0, 0 is the bottom-left of the top-right chunk out of the 4 centre chunks
    }

    private int OppositeQuadrant(int quadrant)
    {
        return quadrant switch
        {
            1 => 4, // Opposite of Q1 is Q4
            2 => 3, // Opposite of Q2 is Q3
            3 => 2, // Opposite of Q3 is Q2
            4 => 1, // Opposite of Q4 is Q1
            _ => -1 // Invalid quadrant

            /*
                Q1  Q2
                Q3  Q4
            */
        };
    }
}