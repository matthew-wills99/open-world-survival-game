using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.Tilemaps;

public class MapController : MonoBehaviour
{
    public Tilemap groundTilemap;
    public Tilemap buildingTilemap;
    public NoiseGenerator noiseGenerator; // map generator script tied to map empty
    public Transform playerTransform; // transform tied to player game object
    public TileIndex tileIndex;

    Dictionary<int, Tile> tiles;

    public Tile grass;
    public Tile sand;
    public Tile water;
    // convert to tileindex dict

    // max accepted values
    [Range(0f, 1f)]
    public float grassThreshold;
    [Range(0f, 1f)]
    public float sandThreshold;

    public int chunkSize = 16;
    public int renderDist = 2;

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
            Debug.Log($"New Chunk: ({chunkCoords.xCoord}, {chunkCoords.yCoord}) : ({tilePosInChunk.xCoord}, {tilePosInChunk.yCoord})");
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
            buildingTilemap.SetTile(cellPos, tiles[selectedTile]);
            return;
        }
        Debug.Log($"Old chunk: ({chunkCoords.xCoord}, {chunkCoords.yCoord}) : ({tilePosInChunk.xCoord}, {tilePosInChunk.yCoord})");
        buildingChunks[chunkCoords.ToString()].chunkTiles[tilePosInChunk.xCoord, tilePosInChunk.yCoord] = selectedTile;
        buildingTilemap.SetTile(cellPos, tiles[selectedTile]);
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
    void DrawGroundTile(int x, int y, int tileIndex)
    {
        Vector3Int pos = new Vector3Int(x, y, 0);
        groundTilemap.SetTile(pos, tiles[tileIndex]);
    }

    // lag when crossing chunk borders
    void RenderGroundChunks()
    {
        Debug.Log("Render ground chunks");
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
                if(!groundChunks.ContainsKey(chunkCoords.ToString()))
                {
                    GenerateChunk(chunkX, chunkY);
                }

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
        Debug.Log("Yeppers");
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
            //RenderBuildingChunks();
            CleanupChunks();
            // update the player chunk coords
            playerChunkCoords = GetChunkCoords(GetPlayerCoords().xCoord, GetPlayerCoords().yCoord);
        }
    }

    void Update()
    {
        TryRender();
        //Debug.Log(chunks.Keys.Count);
    }

    void Start()
    {
        groundChunks = new Dictionary<string, Chunk>();
        buildingChunks = new Dictionary<string, Chunk>();
        groundChunksInRenderDistance = new Dictionary<string, Coords>();
        buildingChunksInRenderDistance = new Dictionary<string, Coords>();
        
        Debug.Log("Getting Tiles");
        tiles = new Dictionary<int, Tile>();
        tiles.Add(-1, null);
        tiles.Add(0, grass);
        tiles.Add(1, sand);
        tiles.Add(2, water);

        playerChunkCoords = GetChunkCoords(GetPlayerCoords().xCoord, GetPlayerCoords().yCoord);
        RenderGroundChunks();
        //RenderBuildingChunks();
    }
}





// chunk size = 16
// tile coords (174, 156)

// chunk coords (tilecoords / chunksize) = (10, 9)