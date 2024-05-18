using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Xml;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.Tilemaps;
using System.Linq;
using System.Dynamic;

public class MapToTiles : MonoBehaviour
{
    public Tilemap tilemap;
    public MapGenerator mapGenerator; // map generator script tied to map empty
    public Transform playerTransform; // transform tied to player game object
    
    public Tile grassTile;
    public Tile sandTile;
    public Tile waterTile;

    // max accepted values
    [Range(0f, 1f)]
    public float grassThreshold;
    [Range(0f, 1f)]
    public float sandThreshold;

    public int chunkSize = 16;
    public int renderDist = 2;

    // key is in the format of Coords.ToString() 
    Dictionary<string, Chunk> chunks;
    Dictionary<string, Coords> chunksInRenderDistance;

    Coords playerChunkCoords;

    public struct Chunk
    {
        public int xCoord;
        public int yCoord;
        public float[,] chunkTiles;

        public Chunk(int x, int y, float[,] chunk)
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

    // should only be called once per chunk forever.
    // once a chunk is generated it should be stored in the chunks dictionary
    void GenerateChunk(int chunkX, int chunkY)
    {
        Coords chunkCoords = new Coords(chunkX, chunkY);
        if(!chunks.ContainsKey(chunkCoords.ToString()))
        {
            int xCoord = chunkX * chunkSize; // tile coords
            int yCoord = chunkY * chunkSize;
            float[,] chunkTiles = new float[chunkSize, chunkSize];

            for(int x = 0; x < chunkSize; x++)
            {
                for(int y = 0; y < chunkSize; y++)
                {
                    chunkTiles[x, y] = mapGenerator.GetTileNoise(xCoord + x, yCoord + y);
                }
            }

            //Debug.Log($"Generated Chunk: ({chunkX}, {chunkY})");
            chunks.Add(chunkCoords.ToString(), new Chunk(chunkX, chunkY, chunkTiles));
            //chunksInRenderDistance.Add(chunkCoords.ToString(), new Chunk(chunkX, chunkY, chunkTiles));
        }
    }

    Coords GetChunkCoords(int x, int y)
    {
        return new Coords(x / chunkSize, y / chunkSize);
    }

    Coords GetPlayerCoords()
    {
        Vector3Int playerCellPos = tilemap.WorldToCell(playerTransform.position);
        return new Coords(playerCellPos.x, playerCellPos.y);
    }

    Coords GetPlayerChunkCoords()
    {
        return GetChunkCoords(GetPlayerCoords().xCoord, GetPlayerCoords().yCoord);
    }

    // x and y coord of tilemap where tile will go
    // noise value to find the correct tile
    void PlaceTile(int x, int y, float value)
    {
        Vector3Int pos = new Vector3Int(x, y, 0);
        if(tilemap.GetTile(pos))
        {
            return;
        }

        //Debug.Log($"tile value at ({x}, {y}) is {value}");

        // TODO: fix this garbage (very very temporary)
        if(value <= grassThreshold)
        {
            tilemap.SetTile(pos, grassTile);
            return;
        }
        if(value <= sandThreshold)
        {
            tilemap.SetTile(pos, sandTile);
            return;
        }
        tilemap.SetTile(pos, waterTile);
        return;
    }

    void RenderChunks()
    {
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

                // if this chunk has not yet been generated, generate it
                if(!chunks.ContainsKey(chunkCoords.ToString()))
                {
                    GenerateChunk(chunkX, chunkY);
                }

                float[,] currentChunk = chunks[chunkCoords.ToString()].chunkTiles;

                // tile coords
                for(int x = 0; x < chunkSize; x++)
                {
                    for(int y = 0; y < chunkSize; y++)
                    {
                        PlaceTile((chunkX * chunkSize) + x, (chunkY * chunkSize) + y, currentChunk[x, y]);
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
        
        chunksInRenderDistance = new Dictionary<string, Coords>();

        for(int x = playerChunkX - renderDist; x < playerChunkX + renderDist; x++)
        {
            for(int y = playerChunkY - renderDist; y < playerChunkY + renderDist; y++)
            {
                chunkCoords = new Coords(x, y);
                chunksInRenderDistance.Add(chunkCoords.ToString(), chunkCoords);
            }
        }

        foreach(var chunk in chunks)
        {
            if(!chunksInRenderDistance.ContainsKey(chunk.Key))
            {
                DisposeChunk(chunk.Value);
            }
        }
    }

    void DisposeChunk(Chunk chunk)
    {
        int chunkX = chunk.xCoord;
        int chunkY = chunk.yCoord;

        for(int x = chunkX * chunkSize; x < chunkX * chunkSize + chunkSize; x++)
        {
            for(int y = chunkY * chunkSize; y < chunkY * chunkSize + chunkSize; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), null);
            }
        }
    }

    void TryRender()
    {
        // if the player has moved into a different chunk
        if(!GetChunkCoords(GetPlayerCoords().xCoord, GetPlayerCoords().yCoord).ToString().Equals(playerChunkCoords.ToString()))
        {
            RenderChunks();
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
        chunks = new Dictionary<string, Chunk>();
        chunksInRenderDistance = new Dictionary<string, Coords>();

        playerChunkCoords = GetChunkCoords(GetPlayerCoords().xCoord, GetPlayerCoords().yCoord);
        RenderChunks();
        //GenerateChunk(0, 0);
    }

}

