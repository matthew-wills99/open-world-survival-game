using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class MapController : MonoBehaviour
{
    public int worldSeed = 420;

    public Tilemap groundTilemap;
    public Tilemap buildingTilemap;
    public NoiseGenerator noiseGenerator; // map generator script tied to map empty
    public StructureGenerator structureGenerator; // structure generator script
    public BetterWaveFunction waveFunction;

    public Transform playerTransform; // transform tied to player game object
    public OldTileIndex tileIndex;

    Dictionary<int, Tile> tiles;

    public Tile grass;
    public Tile sand;
    public Tile water;
    public Tile stone;
    // convert to tileindex dict

    // max accepted values
    [Range(0f, 1f)]
    public float grassThreshold;
    [Range(0f, 1f)]
    public float sandThreshold;

    public int chunkSize = 16;
    public int renderDist = 2;
    public int worldSizeInChunks = 32; // 32 small, 64 medium, 128 large, 256 huge (for now)

    // structure generator stuff start -------------------------------------------------------------------

    public Tilemap tempTilemap;

    // quadrant number, chunk coords of structure
    List<KeyValuePair<int, Coords>> structures;
    /*
    1 is -, +
    2 is -, -
    3 is +, -
    4 is +, +
    */

    public int structuresPerQuadrant = 2; // how many structures in each quadrant of the map (+, +), (-, -), (+, -), (-, +)
    public int minimumDistanceBetweenStructures = 2; // minimum distance between structures in chunks, must be at most (worldSize / 2) - 1
    public int structureRadius = 10;

    /*
    quadrants are
    x < 0, y > 0
    x > 0, y > 0
    x < 0, y < 0
    x > 0, y < 0
    */

    System.Random random;

    // structure generator stuff end ---------------------------------------------------------------------


    // key is in the format of Coords.ToString() 
    Dictionary<string, Chunk> groundChunks;
    Dictionary<string, Chunk> buildingChunks;
    Dictionary<string, Coords> groundChunksInRenderDistance;
    Dictionary<string, Coords> buildingChunksInRenderDistance;

    Coords playerChunkCoords;

    public struct Chunk
    {
        public int xCoord;
        public int yCoord;
        public int[,] chunkTiles;

        public Chunk(int x, int y, int[,] chunk)
        {
            xCoord = x;
            yCoord = y;
            chunkTiles = chunk;
        }
    }

    public struct Coords
    {
        public int xCoord;
        public int yCoord;

        public Coords(int x, int y)
        {
            xCoord = x;
            yCoord = y;
        }

        public override readonly string ToString()
        {
            return $"({xCoord}, {yCoord})";
        }
    }

    public int GetWorldSeed()
    {
        return worldSeed;
    }

    public void PlaceTile(Vector3Int cellPos, int selectedTile)
    {
        // get chunk coordinates from cell pos
        Coords chunkCoords = GetChunkCoords(cellPos.x, cellPos.y);
        // get tile pos in chunk
        Coords tilePosInChunk = new Coords(cellPos.x % chunkSize, cellPos.y % chunkSize);

        if(tilePosInChunk.xCoord < 0)
        {
            tilePosInChunk.xCoord = chunkSize + tilePosInChunk.xCoord;
        }
        if(tilePosInChunk.yCoord < 0)
        {
            tilePosInChunk.yCoord = chunkSize + tilePosInChunk.yCoord;
        }

        //Debug.Log($"tile pos in chunk: ({tilePosInChunk.xCoord}, {tilePosInChunk.yCoord})");

        // if the player has not built in this chunk before
        if(!buildingChunks.ContainsKey(chunkCoords.ToString()))
        {
            //Debug.Log($"New Chunk: ({chunkCoords.xCoord}, {chunkCoords.yCoord}) : ({tilePosInChunk.xCoord}, {tilePosInChunk.yCoord})");
            int[,] chunkTiles = new int[chunkSize, chunkSize];

            // initialize entire array with -1
            for(int x = 0; x < chunkSize; x++)
            {
                for(int y = 0; y < chunkSize; y++)
                {
                    chunkTiles[x, y] = -1;
                }
            }
            chunkTiles[tilePosInChunk.xCoord, tilePosInChunk.yCoord] = selectedTile;
            buildingChunks.Add(chunkCoords.ToString(), new Chunk(chunkCoords.xCoord, chunkCoords.yCoord, chunkTiles));
            buildingTilemap.SetTile(cellPos, tileIndex.GetTileIndex()[selectedTile]);
            Debug.Log($"Placed tile: {selectedTile} in chunk: ({chunkCoords.xCoord}, {chunkCoords.yCoord}) at: ({cellPos.x}, {cellPos.y})");
            return;
        }
        //Debug.Log($"Old chunk: ({chunkCoords.xCoord}, {chunkCoords.yCoord}) : ({tilePosInChunk.xCoord}, {tilePosInChunk.yCoord})");
        buildingChunks[chunkCoords.ToString()].chunkTiles[tilePosInChunk.xCoord, tilePosInChunk.yCoord] = selectedTile;
        if(buildingTilemap != null)
        {
            //Debug.Log("have map");
        }
        if(tileIndex != null)
        {
            //Debug.Log("Have tile index");
        }
        if(tileIndex.GetTileIndex() != null)
        {
            //Debug.Log("Have tile inedx real");
        }
        buildingTilemap.SetTile(cellPos, tileIndex.GetTileIndex()[selectedTile]);
        Debug.Log($"Placed tile: {selectedTile} in chunk: ({chunkCoords.xCoord}, {chunkCoords.yCoord}) at: ({cellPos.x}, {cellPos.y})");
    }

    int GetTileIndex(float noise)
    {
        if(noise <= grassThreshold)
        {
            return 0;
        }
        if(noise <= sandThreshold)
        {
            return 1;
        }
        return 2;
    }

    // should only be called once per chunk forever.
    // once a chunk is generated it should be stored in the chunks dictionary
    void GenerateChunk(int chunkX, int chunkY)
    {
        Coords chunkCoords = new Coords(chunkX, chunkY);
        if(!groundChunks.ContainsKey(chunkCoords.ToString()))
        {
            //Debug.Log($"Generating chunk: {chunkX}, {chunkY}");
            int xCoord = chunkX * chunkSize; // tile coords
            int yCoord = chunkY * chunkSize;
            int[,] chunkTiles = new int[chunkSize, chunkSize];

            // check if the chunk coordinate is outside the world limit bounds
            if(chunkX > worldSizeInChunks || chunkX < -worldSizeInChunks || chunkY > worldSizeInChunks || chunkY < -worldSizeInChunks)
            {
                for(int x = 0; x < chunkSize; x++)
                {
                    for(int y = 0; y < chunkSize; y++)
                    {
                        chunkTiles[x, y] = 2; // water tile
                    }
                }
                groundChunks.Add(chunkCoords.ToString(), new Chunk(chunkX, chunkY, chunkTiles));
                return;
            }

            for(int x = 0; x < chunkSize; x++)
            {
                for(int y = 0; y < chunkSize; y++)
                {
                    chunkTiles[x, y] = GetTileIndex(noiseGenerator.GetTileNoise(xCoord + x, yCoord + y));
                    //Debug.Log($"Chunk[{chunkX}, {chunkY}] ({x}, {y}) = {chunkTiles[x, y]}");
                }
            }

            //Debug.Log($"Generated Chunk: ({chunkX}, {chunkY})");
            groundChunks.Add(chunkCoords.ToString(), new Chunk(chunkX, chunkY, chunkTiles));
        }
    }

    void InitializeBuildingChunks()
    {
        for(int chunkX = -worldSizeInChunks - 1; chunkX <= worldSizeInChunks + 1; chunkX++)
        {
            for(int chunkY = -worldSizeInChunks - 1; chunkY <= worldSizeInChunks + 1; chunkY++)
            {
                Coords chunkCoords = new Coords(chunkX, chunkY);
                if(!buildingChunks.ContainsKey(chunkCoords.ToString()))
                {
                    int[,] chunkTiles = new int[chunkSize, chunkSize];

                    for(int x = 0; x < chunkSize; x++)
                    {
                        for(int y = 0; y < chunkSize; y++)
                        {
                            chunkTiles[x, y] = -1;
                        }
                    }
                    buildingChunks.Add(chunkCoords.ToString(), new Chunk(chunkX, chunkY, chunkTiles));
                }
            }
        }
    }

    public Coords GetTileCoords(Coords chunkCoords, Coords tileInChunk)
    {
        return new Coords(chunkCoords.xCoord * chunkSize + tileInChunk.xCoord, chunkCoords.yCoord * chunkSize + tileInChunk.yCoord);
    }

    public Coords GetChunkCoords(int x, int y)
    {
        if(x < 0 && y < 0)
        {
            return new Coords(x / chunkSize - 1, y / chunkSize - 1);
        }
        if(x < 0)
        {
            return new Coords(x / chunkSize - 1, y / chunkSize);
        }
        if(y < 0)
        {
            return new Coords(x / chunkSize, y / chunkSize - 1);
        }

        return new Coords(x / chunkSize, y / chunkSize);
    }

    public int GetWorldSize()
    {
        return worldSizeInChunks;
    }

    Coords GetPlayerCoords()
    {
        Vector3Int playerCellPos = groundTilemap.WorldToCell(playerTransform.position);
        return new Coords(playerCellPos.x, playerCellPos.y);
    }

    Coords GetPlayerChunkCoords()
    {
        return GetChunkCoords(GetPlayerCoords().xCoord, GetPlayerCoords().yCoord);
    }

    // x and y coord of tilemap where tile will go
    // tile index
    void DrawGroundTile(int x, int y, int tileID)
    {
        Vector3Int pos = new Vector3Int(x, y, 0);
        groundTilemap.SetTile(pos, tileIndex.GetTileIndex()[tileID]);
    }

    // lag when crossing chunk borders
    void RenderGroundChunks()
    {
        //Debug.Log("Render ground chunks");
        Coords playerChunkCoords = GetPlayerChunkCoords();
        
        int playerChunkX = playerChunkCoords.xCoord;
        int playerChunkY = playerChunkCoords.yCoord;
        
        /* 
        from x - render dist to x + render dist: 
        render dist = 3
        x = 0
        from -3 to 3 in x and same for y
        */

        for(int chunkX = playerChunkX - renderDist; chunkX <= playerChunkX + renderDist; chunkX++)
        {
            for(int chunkY = playerChunkY - renderDist; chunkY <= playerChunkY + renderDist; chunkY++)
            {
                Coords chunkCoords = new Coords(chunkX, chunkY);

                //***********************************************************************
                //                         render ground chunks
                //***********************************************************************

                // if this chunk has not yet been generated, generate it
                //if(!groundChunks.ContainsKey(chunkCoords.ToString()))
                //{
                //    GenerateChunk(chunkX, chunkY);
                //}

                int[,] currentGroundChunk = groundChunks[chunkCoords.ToString()].chunkTiles;

                // tile coords
                for(int x = 0; x < chunkSize; x++)
                {
                    for(int y = 0; y < chunkSize; y++)
                    {
                        DrawGroundTile((chunkX * chunkSize) + x, (chunkY * chunkSize) + y, currentGroundChunk[x, y]);
                    }
                }
            }
        }
    }

    // this is causing problems somehow
    void RenderBuildingChunks()
    {
        //Debug.Log("Yeppers");
        Coords playerChunkCoords = GetPlayerChunkCoords();
        
        int playerChunkX = playerChunkCoords.xCoord;
        int playerChunkY = playerChunkCoords.yCoord;

        for(int chunkX = playerChunkX - renderDist; chunkX <= playerChunkX + renderDist; chunkX++)
        {
            for(int chunkY = playerChunkY - renderDist; chunkY <= playerChunkY + renderDist; chunkY++)
            {
                Coords chunkCoords = new Coords(chunkX, chunkY);

                //***********************************************************************
                //                         render building chunks
                //***********************************************************************

                // if there is a building in this chunk
                if(buildingChunks.ContainsKey(chunkCoords.ToString()))
                {
                    int[,] currentBuildingChunk = buildingChunks[chunkCoords.ToString()].chunkTiles;
                    for(int x = 0; x < chunkSize; x++)
                    {
                        for(int y = 0; y < chunkSize; y++)
                        {
                            buildingTilemap.SetTile(new Vector3Int((chunkX * chunkSize) + x, (chunkY * chunkSize) + y, 0), tiles[currentBuildingChunk[x, y]]);
                        }
                    }
                }
            }
        }
    }

    void CleanupChunks()
    {
        Coords playerChunkCoords = GetPlayerChunkCoords();
        //Debug.Log("Player chunk coords are: " + playerChunkCoords.ToString());
        int playerChunkX = playerChunkCoords.xCoord;
        int playerChunkY = playerChunkCoords.yCoord;

        Coords chunkCoords;
        
        groundChunksInRenderDistance = new Dictionary<string, Coords>();
        buildingChunksInRenderDistance = new Dictionary<string, Coords>();

        for(int x = playerChunkX - renderDist; x < playerChunkX + renderDist; x++)
        {
            for(int y = playerChunkY - renderDist; y < playerChunkY + renderDist; y++)
            {
                chunkCoords = new Coords(x, y);
                groundChunksInRenderDistance.Add(chunkCoords.ToString(), chunkCoords);

                if(buildingChunks.ContainsKey(chunkCoords.ToString()))
                {
                    buildingChunksInRenderDistance.Add(chunkCoords.ToString(), chunkCoords);
                }
            }
        }

        foreach(var chunk in groundChunks)
        {
            if(!groundChunksInRenderDistance.ContainsKey(chunk.Key))
            {
                DisposeGroundChunk(chunk.Value);
            }
        }

        foreach(var chunk in buildingChunks)
        {
            if(!buildingChunksInRenderDistance.ContainsKey(chunk.Key))
            {
                DisposeBuildingChunk(chunk.Value);
            }
        }
    }

    void DisposeBuildingChunk(Chunk chunk)
    {
        int chunkX = chunk.xCoord;
        int chunkY = chunk.yCoord;

        for(int x = chunkX * chunkSize; x < chunkX * chunkSize + chunkSize; x++)
        {
            for(int y = chunkY * chunkSize; y < chunkY * chunkSize + chunkSize; y++)
            {
                buildingTilemap.SetTile(new Vector3Int(x, y, 0), tiles[-1]);
            }
        }
    }

    void DisposeGroundChunk(Chunk chunk)
    {
        int chunkX = chunk.xCoord;
        int chunkY = chunk.yCoord;

        for(int x = chunkX * chunkSize; x < chunkX * chunkSize + chunkSize; x++)
        {
            for(int y = chunkY * chunkSize; y < chunkY * chunkSize + chunkSize; y++)
            {
                groundTilemap.SetTile(new Vector3Int(x, y, 0), tiles[-1]);
            }
        }
    }

    void TryRender()
    {
        // if the player has moved into a different chunk
        if(!GetChunkCoords(GetPlayerCoords().xCoord, GetPlayerCoords().yCoord).ToString().Equals(playerChunkCoords.ToString()))
        {
            RenderGroundChunks();
            RenderBuildingChunks();
            CleanupChunks();
            // update the player chunk coords
            playerChunkCoords = GetChunkCoords(GetPlayerCoords().xCoord, GetPlayerCoords().yCoord);
        }
    }

    public void PlaceStructures()
    {
        if(worldSizeInChunks < 32)
        {
            int xx = 0;
            int yy = 0;
            Coords cc = new Coords(xx, yy);
            //buildingChunks[cc.ToString()].chunkTiles[chunkSize / 2, chunkSize / 2] = 3;
            structures.Add(new KeyValuePair<int, Coords>(1, cc));
            waveFunction.GenerateStructure(xx, yy, structureRadius);

            /*
            // smaller worlds get 1 structure per quad
            // -, +
            int structX = random.Next(-worldSizeInChunks, 0 + 1);
            int structY = random.Next(0, worldSizeInChunks + 1);
            Coords cc = new Coords(structX, structY);
            buildingChunks[cc.ToString()].chunkTiles[chunkSize / 2, chunkSize / 2] = 3;
            structures.Add(new KeyValuePair<int, Coords>(1, cc));
            Debug.Log($"Placing a strucure at {structX}, {structY}");
            
            Coords tileInChunk = new Coords(chunkSize / 2, chunkSize / 2);
            Debug.Log("Building structure...");
            waveFunction.GenerateStructure(GetTileCoords(cc, tileInChunk), structureRadius);
            Debug.Log("Built structure");

            // +, +
            structX = random.Next(0, worldSizeInChunks + 1);
            structY = random.Next(0, worldSizeInChunks + 1);
            cc = new Coords(structX, structY);
            buildingChunks[cc.ToString()].chunkTiles[chunkSize / 2, chunkSize / 2] = 3;
            structures.Add(new KeyValuePair<int, Coords>(2, cc));
            Debug.Log($"Placing a strucure at {structX}, {structY}");

            tileInChunk = new Coords(chunkSize / 2, chunkSize / 2);
            Debug.Log("Building structure...");
            waveFunction.GenerateStructure(GetTileCoords(cc, tileInChunk), structureRadius);
            Debug.Log("Built structure");

            // +, -
            structX = random.Next(0, worldSizeInChunks + 1);
            structY = random.Next(-worldSizeInChunks, 0);
            cc = new Coords(structX, structY);
            buildingChunks[cc.ToString()].chunkTiles[chunkSize / 2, chunkSize / 2] = 3;
            structures.Add(new KeyValuePair<int, Coords>(3, cc));
            Debug.Log($"Placing a strucure at {structX}, {structY}");

            tileInChunk = new Coords(chunkSize / 2, chunkSize / 2);
            Debug.Log("Building structure...");
            waveFunction.GenerateStructure(GetTileCoords(cc, tileInChunk), structureRadius);
            Debug.Log("Built structure");

            // -, -
            structX = random.Next(-worldSizeInChunks, 0 + 1);
            structY = random.Next(-worldSizeInChunks, 0);
            cc = new Coords(structX, structY);
            buildingChunks[cc.ToString()].chunkTiles[chunkSize / 2, chunkSize / 2] = 3;
            structures.Add(new KeyValuePair<int, Coords>(4, cc));
            Debug.Log($"Placing a strucure at {structX}, {structY}");

            tileInChunk = new Coords(chunkSize / 2, chunkSize / 2);
            Debug.Log("Building structure...");
            waveFunction.GenerateStructure(GetTileCoords(cc, tileInChunk), structureRadius);
            Debug.Log("Built structure");
            */
        }
        else
        {
            // large worlds get 2 structures per quad
            Debug.Log("i didnt set that up yet lol");
        }
    }

    void GenerateMap()
    {
        Debug.Log("Generating map...");
        Debug.Log("Generating ground...");
        for(int x = -worldSizeInChunks - 1; x <= worldSizeInChunks + 1; x++)
        {
            for(int y = -worldSizeInChunks - 1; y <= worldSizeInChunks + 1; y++)
            {
                Coords chunkCoords = new Coords(x, y);
                if(!groundChunks.ContainsKey(chunkCoords.ToString()))
                {
                    GenerateChunk(x, y);
                }
            }
        }
        Debug.Log("Done generating ground.");
        Debug.Log("Generating structures...");
        Debug.Log("Initializing chunks...");
        InitializeBuildingChunks();
        Debug.Log("Done initializing chunks.");
        Debug.Log("Placing structures...");
        PlaceStructures();
        Debug.Log("All structures placed.");
        Debug.Log("Done generating structures.");
    }

    public int GetStructureOffset()
    {
        return 4;
    }

    void Update()
    { 
        //TryRender();
        //Debug.Log(chunks.Keys.Count);
    }

    void Start()
    {
        random = new System.Random(worldSeed);

        if(minimumDistanceBetweenStructures > (worldSizeInChunks / 2) - 1)
        {
            minimumDistanceBetweenStructures = (worldSizeInChunks / 2) - 1;
        }

        groundChunks = new Dictionary<string, Chunk>();
        buildingChunks = new Dictionary<string, Chunk>();
        groundChunksInRenderDistance = new Dictionary<string, Coords>();
        buildingChunksInRenderDistance = new Dictionary<string, Coords>();

        structures = new List<KeyValuePair<int, Coords>>();
        
        Debug.Log("Getting Tiles");
        /*tiles = new Dictionary<int, Tile>
        {
            { -1, null },
            { 0, grass },
            { 1, sand },
            { 2, water },
            { 3, stone }
        };*/

        playerChunkCoords = GetChunkCoords(GetPlayerCoords().xCoord, GetPlayerCoords().yCoord);

        GenerateMap();
        
        // render entire map at once
        /*Debug.Log("Rendering map...");
        for(int x = -worldSizeInChunks - 1; x <= worldSizeInChunks + 1; x++)
        {
            for(int y = -worldSizeInChunks - 1; y <= worldSizeInChunks + 1; y++)
            {
                Coords chunkCoords = new Coords(x, y);
                Chunk chunk = groundChunks[chunkCoords.ToString()];
                for(int i = 0; i < chunkSize; i ++)
                {
                    for(int j = 0; j < chunkSize; j++)
                    {
                        DrawGroundTile((x * chunkSize) + i, (y * chunkSize) + j, chunk.chunkTiles[i, j]);
                    }
                }
            }
        }
        Debug.Log("Done rendering.");

        Debug.Log("Rendering structures...");
        for(int chunkX = -worldSizeInChunks - 1; chunkX <= worldSizeInChunks + 1; chunkX++)
        {
            for(int chunkY = -worldSizeInChunks - 1; chunkY <= worldSizeInChunks + 1; chunkY++)
            {
                Coords chunkCoords = new Coords(chunkX, chunkY);
                if(buildingChunks.ContainsKey(chunkCoords.ToString()))
                {
                    int[,] currentBuildingChunk = buildingChunks[chunkCoords.ToString()].chunkTiles;
                    for(int x = 0; x < chunkSize; x++)
                    {
                        for(int y = 0; y < chunkSize; y++)
                        {
                            buildingTilemap.SetTile(new Vector3Int((chunkX * chunkSize) + x, (chunkY * chunkSize) + y, 0), tileIndex.GetTileIndex()[currentBuildingChunk[x, y]]);
                        }
                    }
                }
            }
        }
        */
        //RenderGroundChunks(); // TODO: add back render distance lol
    }
}





// chunk size = 16
// tile coords (174, 156)

// chunk coords (tilecoords / chunksize) = (10, 9)