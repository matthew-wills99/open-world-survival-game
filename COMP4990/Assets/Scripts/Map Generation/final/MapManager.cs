using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Utils;

public class MapManager : MonoBehaviour
{
    // the origin of the map is considered the centre 4 chunks
    // (0, 0)
    // (-1, 0)
    // (0, -1)
    // (-1, -1)
    string[] centreChunks;
    

    public TileIndex tileIndex;
    
    // Tilemap layers
    public Tilemap underGroundTilemap;
    public Tilemap groundTilemap;
    public Tilemap aboveGroundTilemap;

    int grassTile = 0;
    int waterTile = 34;
    int forestTile = 32;
    int desertTile = 33;

    // Map seed
    public int seed = 100;
    System.Random random;
    public MapSize mapSize = MapSize.Medium;

    public int smoothingPasses = 7;
    public int neighbourRequirement = 4;

    [Range(0, 100)]
    public int randomFillPercent = 45;
    [Range(0, 100)]
    public int randomFillPercentCentre = 65;
    [Range(0, 100)]
    public int biomeChance = 5;

    // key is biome type, value is minimum distance in chunks from the origin that the biome can be placed
    Dictionary<BiomeEnum, int> biomeDistances;
    // value is maximum amount of biomes allowed
    Dictionary<BiomeEnum, int> biomeAmounts;

    List<Biome> forestBiomes;
    List<Biome> desertBiomes;
    List<Biome> allBiomes;

    public int distanceBetweenBiomes = 32;
    public int maxBiomeSize = 16;


    public int chunkSize = 16;
    public int mapSizeInChunks = 8;

    // Key is Utils.GetChunkKey(chunkX, chunkY), value is Utils.Chunk
    Dictionary<string, Chunk> underGroundChunks;
    Dictionary<string, Chunk> groundChunks;
    Dictionary<string, Chunk> aboveGroundChunks;

    // Populate groundChunks with new chunks full of tiles set to -1
    void InitializeChunks()
    {
        // Start at -mapSizeInChunks so that 0, 0 is somewhere near the middle
        for(int x = -mapSizeInChunks / 2; x < mapSizeInChunks / 2; x++)
        {
            for(int y = -mapSizeInChunks / 2; y < mapSizeInChunks / 2; y++)
            {
                groundChunks.Add(GetChunkKey(x, y), new Chunk(x, y, chunkSize));
                /*
                Add underGroundChunks and aboveGroundChunks here?
                */
            }
        }
    }

    void CellularAutomata()
    {
        int selectedTile;
        int border = mapSizeInChunks / 2;
        int randomNumber;

        // Populate each chunk with random biome tiles
        for(int cx = -mapSizeInChunks / 2; cx < mapSizeInChunks / 2; cx++)
        {
            for(int cy = -mapSizeInChunks / 2; cy < mapSizeInChunks / 2; cy++)
            {
                int[,] currentChunkTiles = groundChunks[GetChunkKey(cx, cy)].chunkTiles;
                for(int x = 0; x < chunkSize; x++)
                {
                    for(int y = 0; y < chunkSize; y++)
                    {
                        randomNumber = random.Next(0, 100);
                        // tiles on the border of the map should be water
                        if((cx == -border && x == 0) || (cx == border - 1 && x == chunkSize - 1) || (cy == -border && y == 0) || (cy == border - 1 && y == chunkSize - 1))
                        {
                            selectedTile = waterTile;
                        }
                        // tile is being placed in one of the centre chunks
                        else if(centreChunks.Contains(GetChunkKey(cx, cy)))
                        {
                            // grass super common in centre chunks
                            if(randomNumber < randomFillPercentCentre)
                            {
                                selectedTile = grassTile;
                            }
                            else
                            {
                                selectedTile = waterTile;
                            }
                        }
                        // check if tile should be grass
                        else if(randomNumber < randomFillPercent)
                        {
                            selectedTile = grassTile;
                        }
                        else
                        {
                            selectedTile = waterTile;
                        }
                    
                        currentChunkTiles[x, y] = selectedTile;
                        groundTilemap.SetTile(ChunkToWorldPos(cx, cy, x, y, chunkSize), tileIndex.GetTile(selectedTile)); // temp
                    }
                }
            }
        }
    }

    void /*IEnumerator*/ SmoothMap(int lowX, int lowY, int highX, int highY, int biomeTile = -1)
    {
        for(int i = 0; i < smoothingPasses; i++)
        {
            for(int cx = lowX; cx < highX; cx++)
            {
                for(int cy = lowY; cy < highY; cy++)
                {
                    for(int x = 0; x < chunkSize; x++)
                    {
                        for(int y = 0; y < chunkSize; y++)
                        {
                            //yield return new WaitForSeconds(0.000f);
                            if(biomeTile == -1)
                            {
                                int neighbourWaterTiles = GetSurroundingTileCount(cx, cy, x, y, waterTile);

                                if(neighbourWaterTiles > neighbourRequirement)
                                {
                                    groundChunks[GetChunkKey(cx, cy)].chunkTiles[x, y] = waterTile;
                                    groundTilemap.SetTile(ChunkToWorldPos(cx, cy, x, y, chunkSize), tileIndex.GetTile(waterTile));
                                }
                                else if(neighbourWaterTiles < neighbourRequirement)
                                {
                                    groundChunks[GetChunkKey(cx, cy)].chunkTiles[x, y] = grassTile;
                                    groundTilemap.SetTile(ChunkToWorldPos(cx, cy, x, y, chunkSize), tileIndex.GetTile(grassTile));
                                }
                            }
                            else
                            {
                                // min and max x and y for chunk not working 
                                // debug log some shit idk
                                // biome.minX not accurate for sometimes
                                if(groundChunks[GetChunkKey(cx, cy)].chunkTiles[x, y] != waterTile)
                                {
                                    int neighbourGrassTiles = GetSurroundingTileCount(cx, cy, x, y, grassTile);
                                
                                    if(neighbourGrassTiles < neighbourRequirement)
                                    {
                                        groundChunks[GetChunkKey(cx, cy)].chunkTiles[x, y] = biomeTile;
                                        groundTilemap.SetTile(ChunkToWorldPos(cx, cy, x, y, chunkSize), tileIndex.GetTile(biomeTile));
                                    }
                                    else if(neighbourGrassTiles > neighbourRequirement)
                                    {
                                        groundChunks[GetChunkKey(cx, cy)].chunkTiles[x, y] = grassTile;
                                        groundTilemap.SetTile(ChunkToWorldPos(cx, cy, x, y, chunkSize), tileIndex.GetTile(grassTile));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    int GetSurroundingTileCount(int cx, int cy, int tx, int ty, int idx)
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
                    if(groundChunks.ContainsKey(GetChunkKey(ncx, ncy)))
                    {
                        if(groundChunks[GetChunkKey(ncx, ncy)].chunkTiles[ntx, nty] == idx)
                        {
                            tileCount++;
                        }
                    }
                    else
                    {
                        if(idx == waterTile)
                        {
                            tileCount++; // if on the edge of the map, treat it as water
                        }
                    }
                }
            }
        }
        return tileCount;
    }

    // this function fucking sucks
    // TODO: make good
    void PlaceBiomes()
    {
        foreach(var b in biomeDistances)
        {
            BiomeEnum biome = b.Key;
            bool swag = false;
            int distance = b.Value;
            int maximumAmount = biomeAmounts[biome];
            int biomesPlaced = 0;
            for(int cx = -mapSizeInChunks / 2; cx < mapSizeInChunks / 2; cx++)
            {
                if(swag)
                {
                    break;
                }
                for(int cy = -mapSizeInChunks / 2; cy < mapSizeInChunks / 2; cy++)
                {
                    if(biomesPlaced >= maximumAmount)
                    {
                        swag = true;
                        break;
                    }
                    // check if the chunk is far enough away for a biome to spawn
                    if((cx <= -1 - distance || cx >= 1 + distance) && (cy <= -1 - distance || cy >= 1 + distance))
                    {
                        if(FindValidBiomeLocation(cx, cy) != null)
                        {
                            //25% chance for a biome to spawn in a chunk
                            if(random.Next(0, 100) < 25)
                            {
                                int tx;
                                int ty;
                                switch(biome)
                                {
                                    case BiomeEnum.Forest:
                                        tx = FindValidBiomeLocation(cx, cy).Item1;
                                        ty = FindValidBiomeLocation(cx, cy).Item2;
                                        forestBiomes.Add(new Biome(BiomeEnum.Forest, cx, cy, tx, ty, forestTile));
                                        allBiomes.Add(new Biome(BiomeEnum.Forest, cx, cy, tx, ty, forestTile));
                                        groundChunks[GetChunkKey(cx, cy)].chunkTiles[tx, ty] = forestTile;
                                        groundTilemap.SetTile(ChunkToWorldPos(cx, cy, tx, ty, chunkSize), tileIndex.GetTile(forestTile));
                                        break;
                                    case BiomeEnum.Desert:
                                        tx = FindValidBiomeLocation(cx, cy).Item1;
                                        ty = FindValidBiomeLocation(cx, cy).Item2;
                                        desertBiomes.Add(new Biome(BiomeEnum.Desert, cx, cy, tx, ty, desertTile));
                                        allBiomes.Add(new Biome(BiomeEnum.Desert, cx, cy, tx, ty, desertTile));
                                        groundChunks[GetChunkKey(cx, cy)].chunkTiles[tx, ty] = desertTile;
                                        groundTilemap.SetTile(ChunkToWorldPos(cx, cy, tx, ty, chunkSize), tileIndex.GetTile(desertTile));
                                        break;
                                }
                                biomesPlaced++;
                            }
                        }
                    }
                }
            }
        }
    }

    void SpreadBiomes()
    {
        int[,] directions = { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } };
        // spread all the biomes
        foreach (Biome biome in allBiomes)
        {
            // x, y, distance
            Queue<Tuple<int, int, int>> queue = new Queue<Tuple<int, int, int>>();
            HashSet<Tuple<int, int>> visited = new HashSet<Tuple<int, int>>();

            Vector3Int biomeWorldPos = ChunkToWorldPos(biome.cx, biome.cy, biome.tx, biome.ty, chunkSize);

            /************************************
                This function uses world pos
            ************************************/

            // queue the starting tile
            queue.Enqueue(Tuple.Create(biomeWorldPos.x, biomeWorldPos.y, 0));
            visited.Add(Tuple.Create(biomeWorldPos.x, biomeWorldPos.y));

            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;

            // spreading biome
            while(queue.Count > 0)
            {
                var current = queue.Dequeue();
                int x = current.Item1;
                int y = current.Item2;
                int dist = current.Item3;

                if(dist < maxBiomeSize + random.Next(0, 3))
                {
                    for(int i = 0; i < directions.GetLength(0); i++)
                    {
                        int nx = x + directions[i, 0];
                        int ny = y + directions[i, 1];
                        var nextTile = Tuple.Create(nx, ny);

                        // if neighbouring tile is on the map and hasnt been visited yet
                        if(nx >= -mapSizeInChunks * chunkSize / 2 && nx < mapSizeInChunks * chunkSize / 2 
                        && ny >= -mapSizeInChunks * chunkSize / 2 && ny < mapSizeInChunks * chunkSize / 2 
                        && !visited.Contains(nextTile))
                        {
                            var wtcp = WorldToChunkPos(nextTile.Item1, nextTile.Item2, chunkSize);
                            int cx = wtcp.Item1;
                            int cy = wtcp.Item2;
                            int tx = wtcp.Item3;
                            int ty = wtcp.Item4;
                            if(groundChunks.ContainsKey(GetChunkKey(cx, cy)))
                            {
                                if(groundChunks[GetChunkKey(cx, cy)].chunkTiles[tx, ty] == grassTile)
                                {
                                    if(cx < minX)
                                    {
                                        minX = cx;
                                    }
                                    if(cx > maxX)
                                    {
                                        maxX = cx;
                                    }
                                    if(cy < minY)
                                    {
                                        minY = cy;
                                    }
                                    if(cy > maxY)
                                    {
                                        maxY = cy;
                                    }
                                    groundChunks[GetChunkKey(cx, cy)].chunkTiles[tx, ty] = biome.tileIndex;
                                    groundTilemap.SetTile(new Vector3Int(nextTile.Item1, nextTile.Item2, 0), tileIndex.GetTile(biome.tileIndex));
                                    visited.Add(nextTile);
                                    queue.Enqueue(Tuple.Create(nx, ny, dist + 1));
                                }
                            }
                        }
                    }
                }
            }

            biome.setMinCX(minX);
            biome.setMaxCX(maxX);
            biome.setMinCY(minY);
            biome.setMaxCY(maxY);
        }
    }

    // return x and y of a valid tile in the chunk
    Tuple<int, int> FindValidBiomeLocation(int cx, int cy)
    {
        List<Tuple<int, int>> waterPositions = new List<Tuple<int, int>>();
        List<Tuple<int, int>> grassPositions = new List<Tuple<int, int>>();
        int[,] ct = groundChunks[GetChunkKey(cx, cy)].chunkTiles;
        for(int x = 0; x < chunkSize - 0; x++)
        {
            for(int y = 0; y < chunkSize - 0; y++)
            {
                if(ct[x, y] == waterTile)
                {
                    waterPositions.Add(new Tuple<int, int>(x, y));
                }
                else
                {
                    grassPositions.Add(new Tuple<int, int>(x, y));
                }
            }
        }

        Tuple<int, int> furthestGrass = null;
        int maxDistance = -1;

        foreach(var gPos in grassPositions)
        {
            int minDistance = int.MaxValue;
            foreach(var wPos in waterPositions)
            {
                int dist = Math.Abs(gPos.Item1 - wPos.Item2) + Math.Abs(gPos.Item2 - wPos.Item2);
                if(dist < minDistance)
                {
                    minDistance = dist;
                }
            }

            if(minDistance >= 15 && minDistance > maxDistance)
            {
                bool farEnough = true;
                foreach(var b in allBiomes)
                {
                    if(GetDistance(b.cx * chunkSize + b.tx, b.cy * chunkSize + b.ty, cx * chunkSize + gPos.Item1, cy * chunkSize + gPos.Item2) < distanceBetweenBiomes)
                    {
                        farEnough = false;
                        break;
                    }
                }
                if(farEnough)
                {
                    maxDistance = minDistance;
                    furthestGrass = gPos;
                }
            }
        }
        return furthestGrass;
    }

    void SmoothBiomes()
    {

        // low x of chunk

        foreach(Biome biome in allBiomes)
        {
            SmoothMap(
                biome.mincx,
                biome.mincy,
                biome.maxcx,
                biome.maxcy,
                biome.tileIndex
            );
        }
    }

    void GenerateMap()
    {
        // 1 Initialize map with chunks
        // 2 Generate map
            // a Generate ground layer
        // 3 Generate biomes
            // a Pick random grass tile outside of specified range
            // b Figure it out
        // 4 Generate structures
            // a Place structures somewhere
            // b Build structures around the origin
        
        InitializeChunks();

        CellularAutomata();
        //StartCoroutine(SmoothMap());
        SmoothMap(-mapSizeInChunks / 2, -mapSizeInChunks / 2, mapSizeInChunks / 2, mapSizeInChunks / 2);

        PlaceBiomes();
        SpreadBiomes();
        SmoothBiomes();
    }

    void Start()
    {
        random = new System.Random(seed);
        underGroundChunks = new Dictionary<string, Chunk>();
        groundChunks = new Dictionary<string, Chunk>();
        aboveGroundChunks = new Dictionary<string, Chunk>();

        forestBiomes = new List<Biome>();
        desertBiomes = new List<Biome>();
        allBiomes = new List<Biome>();

        centreChunks = new string[4] {
            GetChunkKey(0, 0),
            GetChunkKey(-1, 0),
            GetChunkKey(0, -1),
            GetChunkKey(-1, -1)
        };

        switch(mapSize)
        {
            case MapSize.Small:
                mapSizeInChunks = 8;
                biomeChance = 10;
                biomeDistances = new Dictionary<BiomeEnum, int>{
                    {BiomeEnum.Forest, 1},
                    {BiomeEnum.Desert, 1}
                };
                biomeAmounts = new Dictionary<BiomeEnum, int>{
                    {BiomeEnum.Forest, 2},
                    {BiomeEnum.Desert, 1}
                };
                break;
            case MapSize.Medium:
                mapSizeInChunks = 12;
                biomeChance = 8;
                biomeDistances = new Dictionary<BiomeEnum, int>{
                    {BiomeEnum.Forest, 1},
                    {BiomeEnum.Desert, 4}
                };
                biomeAmounts = new Dictionary<BiomeEnum, int>{
                    {BiomeEnum.Forest, 3},
                    {BiomeEnum.Desert, 1}
                };
                break;
            case MapSize.Large:
                mapSizeInChunks = 16;
                biomeChance = 5;
                biomeDistances = new Dictionary<BiomeEnum, int>{
                    {BiomeEnum.Forest, 1},
                    {BiomeEnum.Desert, 6}
                };
                biomeAmounts = new Dictionary<BiomeEnum, int>{
                    {BiomeEnum.Forest, 4},
                    {BiomeEnum.Desert, 2}
                };
                break;
        }
        GenerateMap();
    }
}
