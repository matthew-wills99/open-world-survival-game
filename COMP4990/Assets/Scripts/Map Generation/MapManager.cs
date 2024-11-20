using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Utils;

using Tree = Utils.Tree; // yea

public class MapManager : MonoBehaviour
{
    public PorcupineSpawner porcupineSpawner;
    public GameLoop gameLoopManager;
    public DayNightCycle dayNightCycle;
    public UpdateableBlocks updateableBlocks;
    public PlaceableBlockIndex placeableBlockIndex;
    public InventoryManager inventoryManager;

    public bool generateWorldOnStartup = true;

    // out of 100 rock chances
    [Range(0, 100)]
    public int stoneChance;
    [Range(0, 100)]
    public int copperOreChance;
    [Range(0, 100)]
    public int ironOreChance;
    [Range(0, 100)]
    public int goldOreChance;
    
    // NEEDS TO EQUAL
    //public int numOreChances = 4;

    public SortingOrder playerSortingOrder;

    public RegenRocks regenRocks;

    bool gameLoop = false;
    // the origin of the map is considered the centre 4 chunks
    // (0, 0)
    // (-1, 0)
    // (0, -1)
    // (-1, -1)
    string[] centreChunks;

    public SaveWorldScript saveWorldScript;

    string worldName;

    Dictionary<int, Structure> structures;

    public TileIndex tileIndex;
    public WaterController waterController;
    
    // Tilemap layers
    public Tilemap groundTilemap;
    public Tilemap waterTilemap;
    public Tilemap aboveGroundTilemap;

    const int terrainTile = 35;
    const int waterTile = 34;
    const int forestTile = -1;
    const int desertTile = 33;

    const int plainsBiome = -1;
    const int forestBiome = 1;
    const int desertBiome = 2;

    const int tileSize = 32;

    // Map seed
    public int seed = 100;
    System.Random random;
    public MapSize mapSize = MapSize.Medium;

    public int smoothingPasses = 7;
    public int neighbourRequirement = 4;

    const int environmentObjectPlacementDenominator = 10000; // 10,000 = 100 / 10000 = 1%

    [Range(0, 100)]
    public int randomFillPercent = 45;
    [Range(0, 100)]
    public int randomFillPercentCentre = 65;
    [Range(0, 100)]
    public int biomeChance = 5;
    [Range(0, environmentObjectPlacementDenominator)]
    public int plainsTreeChance = 5;
    [Range(0, environmentObjectPlacementDenominator)]
    public int plainsRockChance = 5;
    [Range(0, environmentObjectPlacementDenominator)]
    public int desertCactusChance = 20;
    [Range(0, environmentObjectPlacementDenominator)]
    public int forestTreeChance = 20;


    // key is biome type, value is minimum distance in chunks from the origin that the biome can be placed
    Dictionary<BiomeEnum, int> biomeDistances;
    // value is maximum amount of biomes allowed
    Dictionary<BiomeEnum, int> biomeAmounts;

    List<Biome> forestBiomes;
    List<Biome> desertBiomes;
    List<Biome> allBiomes;
    List<Structure> allStructures;
    //int[] biomeTiles = {32, 33};

    public int distanceBetweenBiomes = 32;
    public int biomeSizeSmall = 16;
    public int biomeSizeMedium = 32;
    public int biomeSizeLarge = 64;
    public int maxBiomeSize = 32;

    public int distanceBetweenStructures = 64;

    public int chunkSize = 16;
    public int mapSizeInChunks = 8;

    // Key is Utils.GetChunkKey(cx, cy), value is Utils.Chunk
    Dictionary<string, Chunk> groundChunks;
    Dictionary<string, Chunk> aboveGroundChunks;
    Dictionary<string, Chunk> waterChunks;
    Dictionary<string, Chunk> biomeChunks;

    GameObject treeEmpty;
    GameObject rockEmpty;
    GameObject cactusEmpty;
    GameObject saplingEmpty;
    // tree and rock dictionary is a good idea!!!!
    // key is Utils.GetCoordinateKey(cx, cy, tx, ty), value is Utils.Tree
    Dictionary<string, Tree> treeObjects;
    List<RockCluster> rockClusters;
    Dictionary<string, Rock> rockObjects;
    Dictionary<string, Cactus> cactusObjects;

    // Populate groundChunks with new chunks full of tiles set to -1
    void InitializeChunks()
    {
        // Start at -mapSizeInChunks so that 0, 0 is somewhere near the middle
        for(int x = -mapSizeInChunks / 2; x < mapSizeInChunks / 2; x++)
        {
            for(int y = -mapSizeInChunks / 2; y < mapSizeInChunks / 2; y++)
            {
                string key = GetChunkKey(x, y);
                groundChunks.Add(key, new Chunk(x, y, chunkSize));
                waterChunks.Add(key, new Chunk(x, y, chunkSize));
                aboveGroundChunks.Add(key, new Chunk(x, y, chunkSize));
                biomeChunks.Add(key, new Chunk(x, y, chunkSize));
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
                int[,] currentChunkTiles = groundChunks[GetChunkKey(cx, cy)].ChunkTiles;
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
                            // terrain super common in centre chunks
                            if(randomNumber < randomFillPercentCentre)
                            {
                                selectedTile = terrainTile;
                            }
                            else
                            {
                                selectedTile = waterTile;
                            }
                        }
                        // check if tile should be terrain
                        else if(randomNumber < randomFillPercent)
                        {
                            selectedTile = terrainTile;
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
                                    groundChunks[GetChunkKey(cx, cy)].ChunkTiles[x, y] = waterTile;
                                    groundTilemap.SetTile(ChunkToWorldPos(cx, cy, x, y, chunkSize), tileIndex.GetTile(waterTile));
                                }
                                else if(neighbourWaterTiles < neighbourRequirement)
                                {
                                    groundChunks[GetChunkKey(cx, cy)].ChunkTiles[x, y] = terrainTile;
                                    groundTilemap.SetTile(ChunkToWorldPos(cx, cy, x, y, chunkSize), tileIndex.GetTile(terrainTile));
                                }
                            }
                            else
                            {
                                //Debug.Log("Smoothing biome");
                                // min and max x and y for chunk not working 
                                // debug log some shit idk
                                // biome.minX not accurate for sometimes
                                //Debug.Log($"C({cx}, {cy}) T({x}, {y})");
                                if(groundChunks[GetChunkKey(cx, cy)].ChunkTiles[x, y] != waterTile)
                                {
                                    int neighbourBiomeTiles = GetSurroundingTileCount(cx, cy, x, y, biomeTile);
                                    int neighbourTerrainTiles = GetSurroundingTileCount(cx, cy, x, y, terrainTile);

                                    if(neighbourBiomeTiles > neighbourTerrainTiles)
                                    {
                                        groundChunks[GetChunkKey(cx, cy)].ChunkTiles[x, y] = biomeTile;
                                        groundTilemap.SetTile(ChunkToWorldPos(cx, cy, x, y, chunkSize), tileIndex.GetTile(biomeTile));
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
                        if(groundChunks[GetChunkKey(ncx, ncy)].ChunkTiles[ntx, nty] == idx)
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
    // made better 15.8.24
    void PlaceBiomes()
    {
        foreach(var b in biomeDistances)
        {
            BiomeEnum biome = b.Key;
            int maximumAmount = biomeAmounts[biome];
            int biomesPlaced = 0;

            List<(int cx, int cy)> possibleChunks = new List<(int, int)>();
            for(int cx = -mapSizeInChunks / 2; cx < mapSizeInChunks / 2; cx++)
            {
                for(int cy = -mapSizeInChunks / 2; cy < mapSizeInChunks / 2; cy++)
                {
                    possibleChunks.Add((cx, cy)); // initialize a list of all chunk coordinates in the map
                }
            }

            while(biomesPlaced < maximumAmount)
            {
                int randomIndex = random.Next(0, possibleChunks.Count);
                int cx = possibleChunks[randomIndex].cx;
                int cy = possibleChunks[randomIndex].cy;
                if(FindValidBiomeLocation(cx, cy) != null) // if this chunk has at least 1 tile where a biome can spawn i think
                {
                    // we have randomly selected a chunk from the list, and that chunk is able to spawn a biome
                    int tx = FindValidBiomeLocation(cx, cy).Item1;
                    int ty = FindValidBiomeLocation(cx, cy).Item2;
                    Biome placedBiome = null;
                    switch(biome)
                    {
                        case BiomeEnum.Forest:
                            placedBiome = new Biome(BiomeEnum.Forest, cx, cy, tx, ty, forestTile, forestBiome);
                            forestBiomes.Add(placedBiome);
                            groundChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty] = forestTile;
                            biomeChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty] = forestBiome;
                            //groundTilemap.SetTile(ChunkToWorldPos(cx, cy, tx, ty, chunkSize), tileIndex.GetTile(forestTile));
                            break;
                        case BiomeEnum.Desert:
                            placedBiome = new Biome(BiomeEnum.Desert, cx, cy, tx, ty, desertTile, desertBiome);
                            desertBiomes.Add(placedBiome);
                            groundChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty] = desertTile;
                            biomeChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty] = desertBiome;
                            groundTilemap.SetTile(ChunkToWorldPos(cx, cy, tx, ty, chunkSize), tileIndex.GetTile(desertTile));
                            break;
                    }
                    if(placedBiome != null)
                    {
                        allBiomes.Add(placedBiome);
                    }
                    biomesPlaced++;
                }
                else
                {
                    // we have picked a random chunk, but this chunk sucks.
                    possibleChunks.Remove(possibleChunks[randomIndex]);
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

            Vector3Int biomeWorldPos = ChunkToWorldPos(biome.Cx, biome.Cy, biome.Tx, biome.Ty, chunkSize);

            /************************************
                This function uses world pos for some fucking reason
            ************************************/

            // queue the starting tile
            queue.Enqueue(Tuple.Create(biomeWorldPos.x, biomeWorldPos.y, 0));
            visited.Add(Tuple.Create(biomeWorldPos.x, biomeWorldPos.y));

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
                                if(groundChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty] == terrainTile)
                                {
                                    if(biome.TileIndex == -1)
                                    {
                                        biomeChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty] = biome.BiomeIndex;
                                        visited.Add(nextTile);
                                        queue.Enqueue(Tuple.Create(nx, ny, dist + 1));
                                    }
                                    else
                                    {
                                        groundChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty] = biome.TileIndex;
                                        biomeChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty] = biome.BiomeIndex;
                                        groundTilemap.SetTile(new Vector3Int(nextTile.Item1, nextTile.Item2, 0), tileIndex.GetTile(biome.TileIndex));
                                        visited.Add(nextTile);
                                        queue.Enqueue(Tuple.Create(nx, ny, dist + 1));
                                    }
                                    
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    // this function literally just checks for a full chunk of water i think 8.15.24
    // return x and y of a valid tile in the chunk
    Tuple<int, int> FindValidBiomeLocation(int cx, int cy)
    {
        List<Tuple<int, int>> waterPositions = new List<Tuple<int, int>>();
        List<Tuple<int, int>> terrainPositions = new List<Tuple<int, int>>();
        int[,] ct = groundChunks[GetChunkKey(cx, cy)].ChunkTiles;
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
                    terrainPositions.Add(new Tuple<int, int>(x, y));
                }
            }
        }

        // idk wtf this is
        Tuple<int, int> furthestTerrain = null;
        int maxDistance = -1;

        foreach(var gPos in terrainPositions)
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
                    if(GetDistance(b.Cx * chunkSize + b.Tx, b.Cy * chunkSize + b.Ty, cx * chunkSize + gPos.Item1, cy * chunkSize + gPos.Item2) < distanceBetweenBiomes)
                    {
                        farEnough = false;
                        break;
                    }
                }
                if(farEnough)
                {
                    maxDistance = minDistance;
                    furthestTerrain = gPos;
                }
            }
        }
        return furthestTerrain;
    }

    void SmoothBiomes()
    {

        // low x of chunk

        foreach(Biome biome in allBiomes)
        {
            if(biome.TileIndex != -1) // biome tile index is -1 if that biome does not have a tile
            {
                SmoothMap(
                    -mapSizeInChunks / 2,
                    -mapSizeInChunks / 2,
                    mapSizeInChunks / 2,
                    mapSizeInChunks / 2,
                    biome.TileIndex
                );
            }
            
        }
    }

    void TileVariation()
    {
        for(int cx = -mapSizeInChunks / 2; cx < mapSizeInChunks / 2; cx++)
        {
            for(int cy = -mapSizeInChunks / 2; cy < mapSizeInChunks / 2; cy++)
            {
                if(groundChunks.ContainsKey(GetChunkKey(cx, cy)))
                {
                    for(int tx = 0; tx < chunkSize; tx++)
                    {
                        for(int ty = 0; ty < chunkSize; ty++)
                        {
                            switch(groundChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty])
                            {
                                case waterTile:
                                    break;
                                case terrainTile:
                                    int newTile = tileIndex.GetGrassTiles()[random.Next(0, tileIndex.GetGrassTiles().Count)];
                                    groundChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty] = newTile;
                                    groundTilemap.SetTile(ChunkToWorldPos(cx, cy, tx, ty, chunkSize), tileIndex.GetTile(newTile));
                                    break;
                                case desertTile:
                                    break;
                                case forestTile:
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }

    bool IsValidStructureOrigin(int cx, int cy, int tx, int ty, int order)
    {
        if(centreChunks.Contains(GetChunkKey(cx, cy)))
        {
            // we not putting this in the middle of the map u heard
            return false;
        }
        int ncx = cx;
        int ncy = cy;
        int ntx = tx;
        int nty = ty;
        //Debug.Log($"order; {order}");
        for(int x = ntx - structures[order].XRad - 1; x <= ntx + structures[order].XRad + 1; x++)
        {
            for(int y = nty - structures[order].YDown - 1; y <= nty + structures[order].YUp + 2; y++)
            {
                if(x < 0)
                {
                    x = chunkSize + x;
                    ncx--;
                }
                if(x >= chunkSize)
                {
                    x -= chunkSize;
                    ncx++;
                }
                if(y < 0)
                {
                    y = chunkSize + y;
                    ncy--;
                }
                if(y >= chunkSize)
                {
                    y -= chunkSize;
                    ncy++;
                }
                if(groundChunks.ContainsKey(GetChunkKey(ncx, ncy)))
                {
                    //Debug.Log($"{x}, {y}");
                    if(groundChunks[GetChunkKey(ncx, ncy)].ChunkTiles[x, y] != terrainTile)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
        return true;
    }

    void PopulateEnvironment()
    {
        // maybe add environmental objects to tile index??
        for(int cx = -mapSizeInChunks / 2; cx < mapSizeInChunks / 2; cx++)
        {
            for(int cy = -mapSizeInChunks / 2; cy < mapSizeInChunks / 2; cy++)
            {
                for(int tx = 0; tx < chunkSize; tx++)
                {
                    for(int ty = 0; ty < chunkSize; ty++)
                    {
                        if(groundChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty] != waterTile)
                        {
                            // plains biome needs rocks and trees
                            if(biomeChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty] == plainsBiome)
                            {
                                if(random.Next(0, environmentObjectPlacementDenominator) < plainsRockChance)
                                {

                                    if(!TileHasObject(cx, cy, tx, ty))
                                    {
                                        PlaceRockCluster(cx, cy, tx, ty);
                                    }
                                }
                                // i think that rocks should always spawn over trees because rocks only spawn in plains
                                else if(random.Next(0, environmentObjectPlacementDenominator) < plainsTreeChance)
                                {
                                    Tree tempTree = new Tree(tileIndex.GetAllTrees()[random.Next(tileIndex.GetAllTrees().Count)], cx, cy, tx, ty);
                                    treeObjects.Add(GetCoordinateKey(cx, cy, tx, ty), tempTree);
                                    var worldPos = ChunkToWorldPos(cx, cy, tx, ty, chunkSize);
                                    GameObject instance = Instantiate(tileIndex.GetObject(tempTree.Index), new Vector3(worldPos.x + 0.5f, worldPos.y + 0.5f, worldPos.z), quaternion.identity, treeEmpty.transform);
                                    instance.tag = "Selectable";
                                    instance.GetComponent<TreeObj>().SetCoords(cx, cy, tx, ty);
                                }
                            }
                            // desert biome needs cactus
                            else if(biomeChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty] == desertBiome)
                            {
                                if(random.Next(0, environmentObjectPlacementDenominator) < desertCactusChance)
                                {
                                    Cactus tempCactus = new Cactus(tileIndex.GetAllCactus()[random.Next(tileIndex.GetAllCactus().Count)], cx, cy, tx, ty);
                                    cactusObjects.Add(GetCoordinateKey(cx, cy, tx, ty), tempCactus);
                                    var worldPos = ChunkToWorldPos(cx, cy, tx, ty, chunkSize);
                                    GameObject instance = Instantiate(tileIndex.GetObject(tileIndex.GetAllCactus()[random.Next(tileIndex.GetAllCactus().Count)]), new Vector3(worldPos.x + 0.5f, worldPos.y + 0.5f, worldPos.z), quaternion.identity, cactusEmpty.transform);
                                    instance.tag = "Selectable";
                                    instance.GetComponent<TCactusObj>().SetCoords(cx, cy, tx, ty);
                                }
                            }
                            // forest biome needs trees
                            else if(biomeChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty] == forestBiome)
                            {
                                if(random.Next(0, environmentObjectPlacementDenominator) < forestTreeChance)
                                {
                                    Tree tempTree = new Tree(tileIndex.GetAllTrees()[random.Next(tileIndex.GetAllTrees().Count)], cx, cy, tx, ty);
                                    treeObjects.Add(GetCoordinateKey(cx, cy, tx, ty), tempTree);
                                    var worldPos = ChunkToWorldPos(cx, cy, tx, ty, chunkSize);
                                    GameObject instance = Instantiate(tileIndex.GetObject(tempTree.Index), new Vector3(worldPos.x + 0.5f, worldPos.y + 0.5f, worldPos.z), quaternion.identity, treeEmpty.transform);
                                    instance.tag = "Selectable";
                                    instance.GetComponent<TreeObj>().SetCoords(cx, cy, tx, ty);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public bool TileHasWater(int cx, int cy, int tx, int ty)
    {
        // check if water could exist
        if(waterChunks.ContainsKey(GetChunkKey(cx, cy))) 
        {
            //Debug.Log($"Water exists in chunk: {waterChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty]}");
            // check if there is water on that tile
            if(tileIndex.GetAllGrassTiles().Contains(groundChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty]))
            {
                return false;
            }
            return true;
        }
        return false;
    }

    public void PlaceRockCluster(int cx, int cy, int tx, int ty)
    {
        /* need to pick a random type of rock from the types we have
        Stone
        Copper Ore
        Iron Ore
        Gold Ore

        these should also be weighted
        create a weight number for each option
        
        */

        //Debug.Log("--------------Rock cluster START---------------");
        // take this spot, pick random spots around it in a 1 tile radius and place 2 more rocks
        // do not place a rock if there is something in the way, check for water, trees, other rocks, 
        // and above ground tiles
        List<(int x, int y)> options = new List<(int, int)>{
            (-1, -1),
            (-1, 0),
            (-1, 1),
            (0, -1),
            (0, 1),
            (1, -1),
            (1, 0),
            (1, 1)
        };

        RockCluster cluster = new RockCluster();

        // select a type of rock to place
        int rockType = random.Next(0, 100);
        int oreType;
        if(rockType < stoneChance)
        {
            oreType = tileIndex.GetStoneObjIndex();
        }
        else if(rockType < copperOreChance)
        {
            oreType = tileIndex.GetCopperOreObjIndex();
        }
        else if(rockType < ironOreChance)
        {
            oreType = tileIndex.GetIronOreObjIndex();
        }
        else if(rockType < goldOreChance)
        {
            oreType = tileIndex.GetGoldOreObjIndex();
        }
        else
        {
            oreType = 7;
        }

        // place the main rock
        Rock mainRock = new Rock(oreType, cx, cy, tx, ty, cluster);
        rockObjects.Add(GetCoordinateKey(cx, cy, tx, ty), mainRock);
        var worldPos = ChunkToWorldPos(cx, cy, tx, ty, chunkSize);
        GameObject instance = Instantiate(tileIndex.GetObject(mainRock.Index), new Vector3(worldPos.x + 0.5f, worldPos.y + 0.5f, worldPos.z), quaternion.identity, rockEmpty.transform);
        instance.GetComponent<RockObj>().SetCoords(cx, cy, tx, ty);
        instance.tag = "Selectable";

        int rocksPlaced = 0;
        int maxRocks = random.Next(0, 3); // random rocks 0 extra, 1 extra, 2 extra
        (int x, int y) currentTile;
        // convert to world so we can add x and y without checking for chunk overflow
        Vector3Int worldCoords = ChunkToWorldPos(cx, cy, tx, ty, chunkSize);
        Vector3Int newCoords = worldCoords;
        (int cx, int cy, int tx, int ty) newTile;
        
        // as long as we have options to put it
        while(options.Count > 0)
        {
            //Debug.Log($"Options count: {options.Count}");
            newCoords = worldCoords;
            currentTile = options[random.Next(options.Count)];
            int dx = currentTile.x;
            int dy = currentTile.y;
            newCoords.x += dx;
            newCoords.y += dy;
            newTile = WorldToChunkPos(newCoords.x, newCoords.y, chunkSize);
            // check that we wont be placing too many rocks
            if(rocksPlaced <= maxRocks)
            {
                if(!TileHasObject(newTile.cx, newTile.cy, newTile.tx, newTile.ty))
                {
                    if(groundChunks[GetChunkKey(newTile.cx, newTile.cy)].ChunkTiles[newTile.tx, newTile.ty] != -1)
                    {
                        //Debug.Log("\tNo Water");
                        // if there is no above ground tile in the way
                        if(!TileHasWater(newTile.cx, newTile.cy, newTile.tx, newTile.ty))
                        {
                            // place a rock here yippee
                            //Debug.Log("Placed");
                            Rock tempRock = new Rock(oreType, newTile.cx, newTile.cy, newTile.tx, newTile.ty, cluster);
                            rockObjects.Add(GetCoordinateKey(newTile.cx, newTile.cy, newTile.tx, newTile.ty), tempRock);
                            worldPos = ChunkToWorldPos(newTile.cx, newTile.cy, newTile.tx, newTile.ty, chunkSize);
                            instance = Instantiate(tileIndex.GetObject(mainRock.Index), new Vector3(worldPos.x + 0.5f, worldPos.y + 0.5f, worldPos.z), quaternion.identity, rockEmpty.transform);
                            instance.GetComponent<RockObj>().SetCoords(newTile.cx, newTile.cy, newTile.tx, newTile.ty);
                            instance.tag = "Selectable";
                            rocksPlaced += 1;
                            //Debug.Log($"Added child to cluster {cluster.children}");
                        }
                    }
                }
                // regardless of if a rock was placed here or not, we checked it, so it gets removed from the list of options
                options.Remove(currentTile);
            }
            else
            {
                return; // placed max amount of rocks
            }
        }
        //Debug.Log("--------------Rock cluster END-------------------");
        return; // ran out of possible locations for rocks
    }

    void SetupChunkLayers()
    {
        foreach(Chunk chunk in groundChunks.Values)
        {
            for(int tx = 0; tx < chunkSize; tx++)
            {
                for(int ty = 0; ty < chunkSize; ty++)
                {
                    if(chunk.ChunkTiles[tx, ty] == waterTile)
                    {
                        waterTilemap.SetTile(ChunkToWorldPos(chunk.X, chunk.Y, tx, ty, chunkSize), tileIndex.GetTile(waterTile));
                        waterChunks[chunk.GetKey()].ChunkTiles[tx, ty] = waterTile;
                    }
                }
            }
        }
        waterController.Setup(waterTilemap, waterChunks, chunkSize, mapSizeInChunks);
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

        //PlaceStructures(); // KEEP TRACK OF WHAT CHUNK EACH STRUCTURE IS IN AND ONLY ITERATE THROUGH CHUNKS THAT ARE FAR ENOUGH AWAY FOR ANOTHER STRUCTURE

        PlaceBiomes();
        SpreadBiomes(); // KEEP TRACK OF HIGHEST AND LOWEST WORLD COORDINATE THEN CONVERT IT BACK TO CHUNK COORDS
                        // WHEN YOU DO THAT LOOP THROUGH ALL CHUNKS FROM HIGHEST, HIGHEST TO LOWEST, LOWEST X AND Y AND REPLACE ALL BIOME
                        // TILES WITH PURPLE SO U KNOW IT WORKS
        SmoothBiomes();

        TileVariation();
        //TileSplatting();

        PopulateEnvironment();
        SetupChunkLayers();
    }

    void Start()
    {
        if(generateWorldOnStartup)
        {
            GenerateNewWorld("new world", mapSize, seed);
        }
    }

    public void GenerateNewWorld(string wn, MapSize ws, int se)// for menu if not change to start()
    {
        worldName = wn;

        mapSize = ws;// for menu
        seed = se;// for menu

        // need to fix structures so that they generate properly, or set the ground beneath them to be not water

        treeEmpty = new GameObject("trees");
        rockEmpty = new GameObject("rocks");
        cactusEmpty = new GameObject("cactus");
        saplingEmpty = new GameObject("saplings");

        random = new System.Random(seed);
        groundChunks = new Dictionary<string, Chunk>();
        waterChunks = new Dictionary<string, Chunk>();
        aboveGroundChunks = new Dictionary<string, Chunk>();
        biomeChunks = new Dictionary<string, Chunk>();

        //order:  grid, x radius from window not including window, blocks up, blocks down, tile
        // lowest order is lowest area in tiles^2
        structures = new Dictionary<int, Structure>
        {
            {2, new Structure(0, 3, 5, 0)},
            {3, new Structure(1, 4, 2, 4)},
            {1, new Structure(2, 3, 2, 1)}
        };

        forestBiomes = new List<Biome>();
        desertBiomes = new List<Biome>();
        allBiomes = new List<Biome>();
        allStructures = new List<Structure>();

        treeObjects = new Dictionary<string, Tree>();
        rockObjects = new Dictionary<string, Rock>();
        cactusObjects = new Dictionary<string, Cactus>();

        centreChunks = new string[4]
        {
            GetChunkKey(0, 0),
            GetChunkKey(-1, 0),
            GetChunkKey(0, -1),
            GetChunkKey(-1, -1)
        };

        switch(mapSize)
        {
            case MapSize.Small:
                Debug.Log("small world");
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
                smoothingPasses += 2; // ADD OFFSET VARIABLE
                randomFillPercent += 2;
                Debug.Log("medium world");
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
                smoothingPasses += 4;
                randomFillPercent += 4;
                Debug.Log("large world");
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

        gameLoopManager.GenerateStartingMobs();

        inventoryManager.EStart();

        dayNightCycle.Setup();

        StartCoroutine(WaitForWorldReady());

        gameLoop = true;
    }

    private IEnumerator WaitForWorldReady()
    {
        while(!inventoryManager.isLoaded)
        {
            yield return null;
        }
        SaveWorld();
    }

    public void LoadExistingWorld(string wn, WorldData worldData)
    {
        Debug.Log($"Loading existing world: {wn}...");
        treeEmpty = new GameObject("trees");
        rockEmpty = new GameObject("rocks");
        cactusEmpty = new GameObject("cactus");
        saplingEmpty = new GameObject("saplings");


        worldName = wn;
        seed = worldData.Seed;
        mapSize = worldData.WorldSize;
        
        Debug.Log($"Player position: ({worldData.PlayerX}, {worldData.PlayerY}");

        groundChunks = worldData.GroundChunks;
        aboveGroundChunks = worldData.AboveGroundChunks;
        waterChunks = worldData.WaterChunks;

        treeObjects = worldData.Trees;
        rockObjects = worldData.Rocks;
        cactusObjects = worldData.Cacti;

        foreach(Tree tree in treeObjects.Values)
        {
            Vector3Int ctwp = ChunkToWorldPos(tree.Cx, tree.Cy, tree.Tx, tree.Ty, chunkSize);
            Vector3 newPos = new Vector3(ctwp.x + 0.5f, ctwp.y + 0.5f, ctwp.z);
            Instantiate(tileIndex.GetObject(tree.Index), newPos, quaternion.identity, treeEmpty.transform);
        }
        foreach(Rock rock in rockObjects.Values)
        {
            Vector3Int ctwp = ChunkToWorldPos(rock.Cx, rock.Cy, rock.Tx, rock.Ty, chunkSize);
            Vector3 newPos = new Vector3(ctwp.x + 0.5f, ctwp.y + 0.5f, ctwp.z);
            Instantiate(tileIndex.GetObject(rock.Index), newPos, quaternion.identity, rockEmpty.transform);
        }
        foreach(Cactus cactus in cactusObjects.Values)
        {
            Vector3Int ctwp = ChunkToWorldPos(cactus.Cx, cactus.Cy, cactus.Tx, cactus.Ty, chunkSize);
            Vector3 newPos = new Vector3(ctwp.x + 0.5f, ctwp.y + 0.5f, ctwp.z);
            Instantiate(tileIndex.GetObject(cactus.Index), newPos, quaternion.identity, cactusEmpty.transform);
        }

        foreach(Chunk chunk in groundChunks.Values)
        {
            for(int x = 0; x < chunkSize; x++)
            {
                for(int y = 0; y < chunkSize; y++)
                {
                    if(chunk.ChunkTiles[x, y] != -1)
                    {
                        groundTilemap.SetTile(ChunkToWorldPos(chunk.X, chunk.Y, x, y, chunkSize), tileIndex.GetTile(chunk.ChunkTiles[x, y]));
                    }
                }
            }
        }
        foreach(Chunk chunk in waterChunks.Values)
        {
            for(int x = 0; x < chunkSize; x++)
            {
                for(int y = 0; y < chunkSize; y++)
                {
                    if(chunk.ChunkTiles[x, y] != -1)
                    {
                        waterTilemap.SetTile(ChunkToWorldPos(chunk.X, chunk.Y, x, y, chunkSize), tileIndex.GetTile(chunk.ChunkTiles[x, y]));
                    }
                    
                }
            }
        }
        /*foreach(Chunk chunk in aboveGroundChunks.Values)
        {
            for(int x = 0; x < chunkSize; x++)
            {
                for(int y = 0; y < chunkSize; y++)
                {
                    if(chunk.ChunkTiles[x, y] != -1)
                    {
                        aboveGroundTilemap.SetTile(ChunkToWorldPos(chunk.X, chunk.Y, x, y, chunkSize), tileIndex.GetTile(chunk.ChunkTiles[x, y]));
                    }
                }
            }
        }*/

        allStructures = worldData.Structures;

        waterController.Setup(waterTilemap, waterChunks, chunkSize, mapSizeInChunks);

        foreach(PlaceableBlock placeableBlock in worldData.PlaceableBlocks)
        {
            Instantiate(placeableBlockIndex.GetPfbById(placeableBlock.PID), new Vector3(placeableBlock.X + 0.5f, placeableBlock.Y + 0.5f, placeableBlock.Z), Quaternion.identity);
        }

        // porcupine CPorcupine..
        // porcupines placed
        // porcupine cap
        gameLoopManager.LoadExisting(worldData.PorcupineCap, worldData.PorcupinesPlaced);
        foreach(CPorcupine p in worldData.Porcupines)
        {
            Debug.Log("PLACING PORCUPINESSS");
            porcupineSpawner.PlacePorcupine(p.X, p.Y);
        }
        

        dayNightCycle.Load(worldData.DayNightState, worldData.DayNightStateTimer, worldData.DayNightAlpha);

        inventoryManager.LoadInventory(worldData.InventoryItems);

        gameLoop = true;
    }

    void SaveWorld()
    {
        Debug.Log("Attempting to save world...");
        WorldData worldData = new WorldData
        {
            Seed = seed,
            WorldSize = mapSize,
            PlayerX = 0,
            PlayerY = 0,
            WaterChunks = waterChunks,
            GroundChunks = groundChunks,
            AboveGroundChunks = aboveGroundChunks,
            Trees = treeObjects,
            Rocks = rockObjects,
            Cacti = cactusObjects,
            Structures = allStructures,
            Porcupines = porcupineSpawner.GetPorcupinesListForWorldGen(),
            PorcupinesPlaced = gameLoopManager.porcupinesPlaced,
            PorcupineCap = gameLoopManager.porcupineCap,
            DayNightState = dayNightCycle.GetCurrentState(),
            DayNightStateTimer = dayNightCycle.GetTimer(),
            DayNightAlpha = dayNightCycle.GetAlpha(),
            PlaceableBlocks = updateableBlocks.GetPlaceableBlocks(),
            InventoryItems = inventoryManager.GetSavedInventory(),
        };

        saveWorldScript.SaveWorld(worldName, worldData);
    }

    public void SetAboveGroundTile(Vector3Int mousePos, int tileIndex)
    {
        (int cx, int cy, int tx, int ty) c = WorldToChunkPos(mousePos.x, mousePos.y, chunkSize);
        aboveGroundChunks[GetChunkKey(c.cx, c.cy)].ChunkTiles[c.tx, c.ty] = tileIndex;
    }

    public void SetWaterChunks(Dictionary<string, Chunk> wc)
    {
        waterChunks = wc;
    }

    void Update()
    {
        if(gameLoop)
        {
            //UpdateWaterTilemapAnimations();
            if(Input.GetKeyDown(KeyCode.S))
            {
                SaveWorld();
                Debug.Log("Saved");
            }

            playerSortingOrder.UpdateSortingOrder();
        }
    }

    public bool TileHasObject(int x, int y)
    {
        (int cx, int cy, int tx, int ty) = WorldToChunkPos(x, y, chunkSize);
        if(treeObjects.ContainsKey(GetCoordinateKey(cx, cy, tx, ty)))
        {
            return true;
        }
        if(cactusObjects.ContainsKey(GetCoordinateKey(cx, cy, tx, ty)))
        {
            return true;
        }
        if(rockObjects.ContainsKey(GetCoordinateKey(cx, cy, tx, ty)))
        {
            return true;
        }
        return false;
    }

    public bool TileHasObject(int cx, int cy, int tx, int ty)
    {
        if(treeObjects.ContainsKey(GetCoordinateKey(cx, cy, tx, ty)))
        {
            return true;
        }
        if(cactusObjects.ContainsKey(GetCoordinateKey(cx, cy, tx, ty)))
        {
            return true;
        }
        if(rockObjects.ContainsKey(GetCoordinateKey(cx, cy, tx, ty)))
        {
            return true;
        }
        return false;
    }

    public bool TileHasPlacedTile(int x, int y)
    {
        (int cx, int cy, int tx, int ty) = WorldToChunkPos(x, y, chunkSize);
        if(aboveGroundChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty] != -1)
        {
            return true;
        }
        return false;
    }

    public (int cx, int cy, int tx, int ty) GetRandomPoint()
    {
        int cx = UnityEngine.Random.Range(-mapSizeInChunks / 2, mapSizeInChunks / 2);
        int cy = UnityEngine.Random.Range(-mapSizeInChunks / 2, mapSizeInChunks / 2);
        int tx = UnityEngine.Random.Range(0, chunkSize);
        int ty = UnityEngine.Random.Range(0, chunkSize);
        return (cx, cy, tx, ty);
    }

    public void PlaceSapling(GameObject sapling, Vector3 pos)
    {
        Vector3Int intPos = new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z);
        SetAboveGroundTile(intPos, 1); // temporary number
        pos.x += 0.5f;
        pos.y += 0.5f;
        Instantiate(sapling, pos, quaternion.identity, saplingEmpty.transform);
    }

    public void DestroyObj(int cx, int cy, int tx, int ty)
    {
        if(rockObjects.ContainsKey(GetCoordinateKey(cx, cy, tx, ty)))
        {
            rockObjects[GetCoordinateKey(cx, cy, tx, ty)].DestroyRock();
            if(rockObjects[GetCoordinateKey(cx, cy, tx, ty)].ParentCluster.children <= 0)
            {
                //Debug.Log("Clustered destroyed");
                regenRocks.RockClusterDestroyed();
            }
            //Debug.Log("Rock");
            rockObjects.Remove(GetCoordinateKey(cx, cy, tx, ty));
        }
        else if(treeObjects.ContainsKey(GetCoordinateKey(cx, cy, tx, ty)))
        {
            //Debug.Log("Tree");
            treeObjects.Remove(GetCoordinateKey(cx, cy, tx, ty));
        }
        else if(cactusObjects.ContainsKey(GetCoordinateKey(cx, cy, tx, ty)))
        {
            //Debug.Log("Cact");
            cactusObjects.Remove(GetCoordinateKey(cx, cy, tx, ty));
        }
    }

    public void PlaceTree(Vector3 pos)
    {
        (int cx, int cy, int tx, int ty) c = WorldToChunkPos((int)pos.x, (int)pos.y, chunkSize);
        if(treeObjects.ContainsKey(GetCoordinateKey(c.cx, c.cy, c.tx, c.ty)))
        {
            return;
        }
        Tree tempTree = new Tree(tileIndex.GetAllTrees()[random.Next(tileIndex.GetAllTrees().Count)], c.cx, c.cy, c.tx, c.ty);
        treeObjects.Add(GetCoordinateKey(c.cx, c.cy, c.tx, c.ty), tempTree);
        GameObject instance = Instantiate(tileIndex.GetObject(tempTree.Index), new Vector3(pos.x, pos.y, pos.z), quaternion.identity, treeEmpty.transform);
        instance.tag = "Selectable";
        instance.GetComponent<TreeObj>().SetCoords(c.cx, c.cy, c.tx, c.ty);
    }

    public void PlaceCactus(Vector3 pos)
    {
        (int cx, int cy, int tx, int ty) c = WorldToChunkPos((int)pos.x, (int)pos.y, chunkSize);
        if(cactusObjects.ContainsKey(GetCoordinateKey(c.cx, c.cy, c.tx, c.ty)))
        {
            return;
        }
        Cactus tempCactus = new Cactus(tileIndex.GetAllCactus()[random.Next(tileIndex.GetAllCactus().Count)], c.cx, c.cy, c.tx, c.ty);
        cactusObjects.Add(GetCoordinateKey(c.cx, c.cy, c.tx, c.ty), tempCactus);
        GameObject instance = Instantiate(tileIndex.GetObject(tempCactus.Index), new Vector3(pos.x, pos.y + 0.5f, pos.z), quaternion.identity, cactusEmpty.transform);
        instance.tag = "Selectable";
        instance.GetComponent<TCactusObj>().SetCoords(c.cx, c.cy, c.tx, c.ty);
    }
}