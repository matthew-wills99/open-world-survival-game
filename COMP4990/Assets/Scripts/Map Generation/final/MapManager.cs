using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Utils;

using Tree = Utils.Tree; // yea

public class MapManager : MonoBehaviour
{
    // the origin of the map is considered the centre 4 chunks
    // (0, 0)
    // (-1, 0)
    // (0, -1)
    // (-1, -1)
    string[] centreChunks;

    Dictionary<int, Structure> structures;
    public Grid struct1;
    public Grid struct2;
    public Grid struct3;

    public GameObject tree1;
    public GameObject tree2;
    public GameObject tree3;

    public List<GameObject> trees;
    
    public GameObject bigRock;
    public GameObject rock1;
    public GameObject rock2;
    public GameObject rock3;
    public GameObject rock4;
    public GameObject rock5;

    public List<GameObject> rockSprites;

    public TileIndex tileIndex;
    
    // Tilemap layers
    public Tilemap underGroundTilemap;
    public Tilemap groundTilemap;
    public Tilemap aboveGroundTilemap;

    const int terrainTile = 35;
    const int waterTile = 34;
    const int forestTile = 32;
    const int desertTile = 33;

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
    [Range(0, 100)]
    public int plainsTreeChance = 5;
    [Range(0, 100)]
    public int plainsRockChance = 5;
    [Range(0, 100)]
    public int desertCactusChance = 20;
    [Range(0, 100)]
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

    //int tileSize = 32;

    public int chunkSize = 16;
    public int mapSizeInChunks = 8;

    // Key is Utils.GetChunkKey(cx, cy), value is Utils.Chunk
    Dictionary<string, Chunk> underGroundChunks;
    Dictionary<string, Chunk> groundChunks;
    Dictionary<string, Chunk> aboveGroundChunks;

    GameObject treeEmpty;
    GameObject rockEmpty;
    GameObject cactusEmpty;
    // tree and rock dictionary is a good idea!!!!
    // key is Utils.GetCoordinateKey(cx, cy, tx, ty), value is Utils.Tree
    Dictionary<string, Tree> treeObjects;
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
                                    groundChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty] = forestTile;
                                    groundTilemap.SetTile(ChunkToWorldPos(cx, cy, tx, ty, chunkSize), tileIndex.GetTile(forestTile));
                                    break;
                                case BiomeEnum.Desert:
                                    tx = FindValidBiomeLocation(cx, cy).Item1;
                                    ty = FindValidBiomeLocation(cx, cy).Item2;
                                    desertBiomes.Add(new Biome(BiomeEnum.Desert, cx, cy, tx, ty, desertTile));
                                    allBiomes.Add(new Biome(BiomeEnum.Desert, cx, cy, tx, ty, desertTile));
                                    groundChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty] = desertTile;
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
                This function uses world pos
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
                                    groundChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty] = biome.TileIndex;
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
            SmoothMap(
                -mapSizeInChunks / 2,
                -mapSizeInChunks / 2,
                mapSizeInChunks / 2,
                mapSizeInChunks / 2,
                biome.TileIndex
            );
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

    /*int CheckNeighbour(int cx, int cy, int tx, int ty)
    {
        if(tx < 0 && cx > -mapSizeInChunks / 2)
        {
            cx -= 1;
            tx = chunkSize + tx;
        }
        if(tx >= chunkSize && cx < mapSizeInChunks / 2)
        {
            cx += 1;
            tx = tx - chunkSize;
        }
        if(ty < 0 && cy > -mapSizeInChunks / 2)
        {
            cy -= 1;
            ty = chunkSize + ty;
        }
        if(ty >= chunkSize && cy < mapSizeInChunks / 2)
        {
            cy += 1;
            ty = ty - chunkSize;
        }

        return groundChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty];
    }*/

    /*BBTKey GetBBT(int cx, int cy, int tx, int ty)
    {
        List<int> allGrass = tileIndex.GetAllGrassTiles();
        int left = CheckNeighbour(cx, cy, tx - 1, ty); // left
        int right = CheckNeighbour(cx, cy, tx + 1, ty); // right
        int up = CheckNeighbour(cx, cy, tx, ty + 1); // up 
        int down = CheckNeighbour(cx, cy, tx, ty - 1); // down
        if(allGrass.Contains(up) && allGrass.Contains(left)) // up left only
        {
            return BBTKey.BlendUpLeft;
        }
        if(allGrass.Contains(up) && allGrass.Contains(right)) // up right only
        {
            return BBTKey.BlendUpRight;
        }
        if(allGrass.Contains(down) && allGrass.Contains(left)) // down left only
        {
            return BBTKey.BlendDownLeft;
        }
        if(allGrass.Contains(down) && allGrass.Contains(right)) // down right only
        {
            return BBTKey.BlendDownRight;
        }
        if(allGrass.Contains(left))
        {
            return BBTKey.BlendLeft;
        }
        if(allGrass.Contains(right))
        {
            return BBTKey.BlendRight;
        }
        if(allGrass.Contains(up))
        {
            return BBTKey.BlendUp;
        }
        if(allGrass.Contains(down))
        {
            return BBTKey.BlendDown;
        }
        return BBTKey.Null;
    }*/

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

    void PlaceStructures()
    {
        List<int> structuresToPlace = new List<int>();
        if(mapSize == MapSize.Small)
        {
            structuresToPlace.Add(1);
            structuresToPlace.Add(2);
        }
        else
        {
            structuresToPlace.Add(1);
            structuresToPlace.Add(2);
            structuresToPlace.Add(3);
        }
        /*
        top left: (-mapSizeInChunks / 2, mapSizeInChunks / 2)
        top right: (mapSizeInChunks / 2, mapSizeInChunks / 2)
        bottom left: (-mapSizeInChunks / 2, -mapSizeInChunks / 2)
        bottom right (mapSizeInChunks / 2, -mapSizeInChunks / 2)
        */

        // structures should be x distance from each other
        /*
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
        */

        //int placedStructures = 0;
        //int maxStructures = structuresToPlace.Count;

        foreach(int order in structuresToPlace)
        {
            bool swag = false;
            for(int cx = -mapSizeInChunks / 2; cx < mapSizeInChunks / 2; cx++)
            {
                if(swag)
                {
                    break;
                }
                for(int cy = -mapSizeInChunks / 2; cy < mapSizeInChunks / 2; cy++)
                {
                    if(swag)
                    {
                        break;
                    }
                    for(int tx = 0; tx < chunkSize; tx++)
                    {
                        if(swag)
                        {
                            break;
                        }
                        for(int ty = 0; ty < chunkSize; ty++)
                        {
                            if(swag)
                            {
                                break;
                            }
                            if(IsValidStructureOrigin(cx, cy, tx, ty, 1))
                            {
                                bool farEnough = true;
                                foreach(var s in allStructures)
                                {
                                    Vector3Int sPos = ChunkToWorldPos(s.Cx, s.Cy, s.Tx, s.Ty, chunkSize);
                                    Vector3Int currPos = ChunkToWorldPos(cx, cy, tx, ty, chunkSize);
                                    if(GetDistance(currPos.x, currPos.y, sPos.x, sPos.y) < distanceBetweenStructures)
                                    {
                                        farEnough = false;
                                    }
                                }
                                if(farEnough)
                                {
                                    Instantiate(structures[order].StructGrid, ChunkToWorldPos(cx, cy, tx, ty, chunkSize), quaternion.identity);
                                    structures[order].Cx = cx;
                                    structures[order].Cy = cy;
                                    structures[order].Tx = tx;
                                    structures[order].Ty = ty;
                                    allStructures.Add(structures[order]);
                                    //placedStructures ++;
                                    swag = true;
                                    //Debug.Log("SWAG!!!!!");
                                }
                            }
                        }
                    }
                }
            }
        } 

        //int cx = random.Next(-mapSizeInChunks / 2, mapSizeInChunks / 2);
        //int cy = random.Next(-mapSizeInChunks / 2, mapSizeInChunks / 2);
        //Vector3Int v3i = ChunkToWorldPos(cx, cy, chunkSize / 2, chunkSize / 2, chunkSize);
        //Instantiate(struct1, new Vector3(v3i.x, v3i.y, v3i.z), Quaternion.identity);
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
                            if(tileIndex.GetAllGrassTiles().Contains(groundChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty]))
                            {
                                if(random.Next(0, 100) < plainsRockChance)
                                {
                                    // rocks will be clusters.
                                    Rock tempRock = new Rock(bigRock, cx, cy, tx, ty);
                                    rockObjects.Add(GetCoordinateKey(cx, cy, tx, ty), tempRock);
                                    Instantiate(tempRock.RockObject, ChunkToWorldPos(cx, cy, tx, ty, chunkSize), quaternion.identity, rockEmpty.transform);
                                }
                                // i think that rocks should always spawn over trees because rocks only spawn in plains
                                else if(random.Next(0, 100) < plainsTreeChance)
                                {
                                    Instantiate(trees[random.Next(trees.Count)], ChunkToWorldPos(cx, cy, tx, ty, chunkSize), quaternion.identity, treeEmpty.transform);
                                }
                            }
                            // desert biome needs cactus
                            else if(groundChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty] == desertTile)
                            {
                                if(random.Next(0, 100) < desertCactusChance)
                                {
                                    Instantiate(trees[random.Next(trees.Count)], ChunkToWorldPos(cx, cy, tx, ty, chunkSize), quaternion.identity, cactusEmpty.transform);
                                }
                            }
                            // forest biome needs trees
                            else if(groundChunks[GetChunkKey(cx, cy)].ChunkTiles[tx, ty] == forestTile)
                            {
                                if(random.Next(0, 100) < forestTreeChance)
                                {
                                    Instantiate(trees[random.Next(trees.Count)], ChunkToWorldPos(cx, cy, tx, ty, chunkSize), quaternion.identity, treeEmpty.transform);
                                }
                            }
                        }
                    }
                }
            }
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

        PlaceStructures(); // KEEP TRACK OF WHAT CHUNK EACH STRUCTURE IS IN AND ONLY ITERATE THROUGH CHUNKS THAT ARE FAR ENOUGH AWAY FOR ANOTHER STRUCTURE

        PlaceBiomes();
        SpreadBiomes(); // KEEP TRACK OF HIGHEST AND LOWEST WORLD COORDINATE THEN CONVERT IT BACK TO CHUNK COORDS
                        // WHEN YOU DO THAT LOOP THROUGH ALL CHUNKS FROM HIGHEST, HIGHEST TO LOWEST, LOWEST X AND Y AND REPLACE ALL BIOME
                        // TILES WITH PURPLE SO U KNOW IT WORKS
        SmoothBiomes();

        TileVariation();
        //TileSplatting();

        PopulateEnvironment();
    }

    public void Startup(MapSize ws, int se)// for menu if not change to start()
    {
        mapSize = ws;// for menu
        seed = se;// for menu

        // need to fix structures so that they generate properly, or set the ground beneath them to be not water

        treeEmpty = new GameObject("trees");
        rockEmpty = new GameObject("rocks");
        cactusEmpty = new GameObject("cactus");

        random = new System.Random(seed);
        underGroundChunks = new Dictionary<string, Chunk>();
        groundChunks = new Dictionary<string, Chunk>();
        aboveGroundChunks = new Dictionary<string, Chunk>();

        //order:  grid, x radius from window not including window, blocks up, blocks down, tile
        // lowest order is lowest area in tiles^2
        structures = new Dictionary<int, Structure>
        {
            {2, new Structure(struct1, 3, 5, 0)},
            {3, new Structure(struct2, 4, 2, 4)},
            {1, new Structure(struct3, 3, 2, 1)}
        };

        forestBiomes = new List<Biome>();
        desertBiomes = new List<Biome>();
        allBiomes = new List<Biome>();
        allStructures = new List<Structure>();

        trees = new List<GameObject>() {
            tree1,
            tree2, // stupid
            tree3
        };

        rockSprites = new List<GameObject>() {
            rock1,
            rock2,
            rock3, // rename this garbage
            rock4,
            rock5,
        };

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

/*

// if 
find all biome-bordering tiles (make sure to exclude water tiles (probably could just have an array of biome bordering tile indexes))
// maybe make bbt object with x, y, and nx, ny from the function thingy
if bbt (biome-bordering tiles) > 0
store all bbt in an array
loop through all bbt
    > switch(bbt dirs)
    case: direction direction
        feed into splatting algorithm

// splatting algo
    loop through the pixels in the area of pixels that are within requirements to be splatted
    // based on going outwards from the column / row of pixels nearest the border with a biome tile  ...
    // ... from outward there is 90% chance a pixel stays original colour, then 80, then 70, then 60 then 50 then 40 then 30 then 20 then 10 u know
that tile will still be considered a tile of whatever it was before it underwent the splatting

when you break a tile that is splatted, re run this entire algorithm again on the neighbouring tiles that could need to be updated
*/

/*
trees can spawn in forest and plains
    plains biome tree chance: 5%
    forest biome tree chance: 35%
desert biomes can spawn cactus
plains biome can spawn rocks
*/