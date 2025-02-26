using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

using static Utils;

[CreateAssetMenu(fileName = "MapGenerator", menuName = "Map Generator")]
public class MapGenerator : ScriptableObject
{
    /*
    MapGenerator.cs will be responsible only for creating a new map. It will not be used to draw anything.

    To generate a map:

    1. Cellular automata to determine what is terrain and what is water

    2. Determine where any biomes will be
        - type of biome (desert, forest, etc..)
        - x distance away from other biomes
        - heightmap?

    3. Populate environment
        - place environment objects (rocks, ores, trees, etc..)
        - depends on the biome

    What will we need?
        - we will store blocks in chunks, chunks will be 8 x 8 blocks
        - chunks will be a 2d array of integers that represent the IDs of blocks
        - all the chunks will be stored in a dictionary, the key will be the x and y coordinate of the chunk.
            - stored in a long
    */

    private Dictionary<long, int[,]> chunks;
    private List<long> centreChunks;
    private Dictionary<long, Biome[,]> biomeMap;
    private Dictionary<long, ERiver[,]> riverMap;
    private Dictionary<long, EObstacle[,]> obstacleMap;
    
    public const int chunkSize = 8; // 8 x 8 chunk
    public const int maxMapSizeInChunks = 512; // 512 x 512 chunk map is the largest allowed

    //settings
    public int seed;
    [Range(2, maxMapSizeInChunks)] public int mapSizeInChunks;
    [SerializeField, ReadOnly] public int mapSizeInBlocks;
    [SerializeField, ReadOnly] public int halfMap;

    [Range(0, 100)] public int centreTerrainThreshold;
    [Range(0, 100)] public int regularTerrainThreshold;

    [Range(1, 10)] public int smoothingPasses;

    private void OnValidate()
    {
        mapSizeInChunks = Mathf.Clamp(mapSizeInChunks, 2, maxMapSizeInChunks);
        
        if(mapSizeInChunks % 2 != 0)
        {
            mapSizeInChunks++;
        }

        mapSizeInBlocks = mapSizeInChunks * chunkSize;
        halfMap = mapSizeInChunks / 2;
    }

    public int GetChunkSize()
    {
        return chunkSize;
    }

    public Dictionary<long, int[,]> GenerateMap()
    {
        System.Random random = new System.Random(seed);

        chunks = new Dictionary<long, int[,]>();
        centreChunks = new List<long>();
        biomeMap = GameManager.Instance.biomeGenerator.GenerateBiomeMap(random);

        InitializeChunks();

        (riverMap, obstacleMap) = GameManager.Instance.riverGenerator.GenerateRiverMap(random);

        CellularAutomata(random);
        SmoothMap(smoothingPasses);

        ShowRiver();

        //After smoothing the map we should swag on em (change the blocks to represent the biomes)
        BlockVariation();
        //Populate environment

        return chunks;
    }

    public List<long> GetCentreChunks()
    {
        return centreChunks;
    }

    private void ShowRiver()
    {
        for(int cx = -halfMap; cx < halfMap; cx++)
        {
            for(int cy = -halfMap; cy < halfMap; cy++)
            {
                int[,] currentChunkBlocks = chunks[GetChunkKey(cx, cy)];

                for(int tx = 0; tx < chunkSize; tx++)
                {
                    for(int ty = 0; ty < chunkSize; ty++)
                    {
                        if(obstacleMap[GetChunkKey(cx, cy)][tx, ty] == EObstacle.Obstacle)
                        {
                            currentChunkBlocks[tx, ty] = 1;
                        }
                        if(riverMap[GetChunkKey(cx, cy)][tx, ty] == ERiver.River)
                        {
                            currentChunkBlocks[tx, ty] = 3;
                        }
                    }
                }
            }
        }
    }

    private void BlockVariation()
    {
        for(int cx = -halfMap; cx < halfMap; cx++)
        {
            for(int cy = -halfMap; cy < halfMap; cy++)
            {
                for(int bx = 0; bx < chunkSize; bx++)
                {
                    for(int by = 0; by < chunkSize; by++)
                    {
                        if(BlockManager.Instance.blockIndex.GetTerrainBlocks().Contains(chunks[GetChunkKey(cx, cy)][bx, by]))
                        {
                            switch(biomeMap[GetChunkKey(cx, cy)][bx, by].biome)
                            {
                                case EBiome.Forest:
                                    chunks[GetChunkKey(cx, cy)][bx, by] = 4;
                                    break;
                                case EBiome.Desert:
                                    chunks[GetChunkKey(cx, cy)][bx, by] = 2;
                                    break;
                                case EBiome.Snow:
                                    chunks[GetChunkKey(cx, cy)][bx, by] = 5;
                                    break;
                                case EBiome.Wasteland:
                                    chunks[GetChunkKey(cx, cy)][bx, by] = 6;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }

    private void InitializeChunks()
    {
        /* 
        Add all chunks to the dictionary
        Initialize all blocks in each chunk to 0 (empty)

        Determine the centre chunks, used for unique map generation later on
        */

        int centreSize = Mathf.Max(2, mapSizeInChunks / 4);

        int centreBottomLeftX = 0 - centreSize / 2;
        int centreBottomLeftY = 0 - centreSize / 2;
        /*
            XXXXX
            XXXXX
            XXXXX
            XXXXX
         -> OXXXX

        This diagram represents all the chunks in the centre,  nd the chunk shown is where the coordinates point
        */

        for(int cx = -halfMap; cx < halfMap; cx++)
        {
            for(int cy = -halfMap; cy < halfMap; cy++)
            {
                chunks.Add(GetChunkKey(cx, cy), InitializeBlockArray(chunkSize));

                // Centre chunks
                if (cx >= centreBottomLeftX && cx < centreBottomLeftX + centreSize && cy >= centreBottomLeftY && cy < centreBottomLeftY + centreSize)
                {
                    centreChunks.Add(GetChunkKey(cx, cy));
                }
            }
        }   
    }

    private int[,] InitializeBlockArray(int chunkSize)
    {
        int[,] riverArray = new int[chunkSize, chunkSize];
        for(int x = 0; x < chunkSize; x++)
        {
            for(int y = 0; y < chunkSize; y++)
            {
                riverArray[x, y] = 2;
            }
        }
        return riverArray;
    }

    private void CellularAutomata(System.Random random)
    {
        /*
        Determine which blocks will be terrain and which blocks will be water

        Blocks on the border of the map will always be water
        Terrain will be generated differently in the centre of the map, 
        */

        int selectedBlock;
        int r; // random number

        for(int cx = -halfMap; cx < halfMap; cx++)
        {
            for(int cy = -halfMap; cy < halfMap; cy++)
            {
                int[,] currentChunkBlocks = chunks[GetChunkKey(cx, cy)];

                for(int x = 0; x < chunkSize; x++)
                {
                    for(int y = 0; y < chunkSize; y++)
                    {
                        r = random.Next(0, 100);

                        // If a block falls on the border of the map, it should be water.
                        if((cx == -halfMap && x == 0) || (cx == halfMap-1 && x == chunkSize-1) || (cy == -halfMap && y == 0) || (cy == halfMap-1 && y == chunkSize-1))
                        {
                            selectedBlock = 3;
                        }
                        /*else if(riverMap.ContainsKey(GetChunkKey(cx, cx)) && riverMap[GetChunkKey(cx, cy)][x, y] == ERiver.River)
                        {
                            selectedBlock = 3;
                        }*/
                        // If we are working within a centre chunk, it is generated differently.
                        else if(centreChunks.Contains(GetChunkKey(cx, cy)))
                        {
                            if(r < centreTerrainThreshold)
                            {
                                selectedBlock = 1;
                            }
                            else
                            {
                                selectedBlock = 3;
                            }
                        }
                        // biome chunks (not Plains)
                        else if(biomeMap.ContainsKey(GetChunkKey(cx, cy)) && biomeMap[GetChunkKey(cx, cy)][x, y].biome != EBiome.Plains)
                        {
                            if(r < biomeMap[GetChunkKey(cx, cy)][x, y].terrainChance)
                            {
                                selectedBlock = 1;
                            }
                            else
                            {
                                selectedBlock = 3;
                            }
                        }
                        // Remaining chunks
                        else if(r < regularTerrainThreshold)
                        {
                            selectedBlock = 1;
                        }
                        else
                        {
                            selectedBlock = 3;
                        }

                        currentChunkBlocks[x, y] = selectedBlock;
                    }
                }
            }
        }
    }

    private void SmoothMap(int smoothingPasses)
    {
        // I LOVE NESTED FOR LOOPS
        for(int pass = 0; pass < smoothingPasses; pass++)
        {
            for(int cx = -halfMap; cx < halfMap; cx++)
            {
                for(int cy = -halfMap; cy < halfMap; cy++)
                {
                    for(int x = 0; x < chunkSize; x++)
                    {
                        for(int y = 0; y < chunkSize; y++)
                        {
                            // 3 is water
                            int neighbourWaterTiles = GetSurroundingTileCount(cx, cy, x, y, 3);

                            // 8 neighbours (4 directions)
                            if(neighbourWaterTiles > 4)
                            {
                                chunks[GetChunkKey(cx, cy)][x, y] = 3;
                            }
                            else if(neighbourWaterTiles < 4)
                            {
                                chunks[GetChunkKey(cx, cy)][x, y] = 1;
                            }
                        }
                    }
                }
            }
        }
    }

    private int GetSurroundingTileCount(int cx, int cy, int tx, int ty, int idx)
    {
        int tileCount = 0;
        int ncx;
        int ncy;
        int ntx;
        int nty;

        for(int x = tx - 1; x <= tx + 1; x++)
        {
            for(int y = ty - 1; y <= ty + 1; y++)
            {
                // ignore the original tile
                if(x != tx || y != ty)
                {
                    ncx = cx;
                    ncy = cy;
                    ntx = x;
                    nty = y;
                    // check if the neighbouring tile is in a different chunk
                    if(x < 0 || x >= chunkSize || y < 0 || y >= chunkSize)
                    {
                        if(x < 0)
                        {
                            ncx--;
                            ntx = chunkSize + x;
                        }
                        else if(x >= chunkSize)
                        {
                            ncx++;
                            ntx = x - chunkSize;
                        }
                        if(y < 0)
                        {
                            ncy--;
                            nty = chunkSize + y;
                        }
                        else if(y >= chunkSize)
                        {
                            ncy++;
                            nty = y - chunkSize;
                        }
                    }
                    // if we are trying to access a chunk and it exists
                    if(chunks.ContainsKey(GetChunkKey(ncx, ncy)))
                    {
                        if(chunks[GetChunkKey(ncx, ncy)][ntx, nty] == idx)
                        {
                            tileCount++;
                        }
                    }
                    // chunk doesn't exist, must be the edge of the map
                    else
                    {
                        // 3 is water, if we are looking for neighbouring water, be sure to treat the edge of the map as water
                        if(idx == 3)
                        {
                            tileCount++; // if on the edge of the map, treat it as water
                        }
                    }
                }
            }
        }
        return tileCount;
    }
}
