using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Xml;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.Tilemaps;

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
    Dictionary<string, float[,]> chunks;

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
            float[,] chunk = new float[chunkSize, chunkSize];

            for(int x = 0; x < chunkSize; x++)
            {
                for(int y = 0; y < chunkSize; y++)
                {
                    chunk[x, y] = mapGenerator.GetTileNoise(xCoord + x, yCoord + y);
                }
            }

            //Debug.Log($"Generated Chunk: ({chunkX}, {chunkY})");
            chunks.Add(chunkCoords.ToString(), chunk);
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

    // x and y coord of tilemap where tile will go
    // noise value to find the correct tile
    void PlaceTile(int x, int y, float value)
    {
        Vector3Int pos = new Vector3Int(x, y, 0);
        if(tilemap.GetTile(pos))
        {
            return;
        }

        Debug.Log($"tile value at ({x}, {y}) is {value}");

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
        // get player tile coords
        Coords playerCoords = GetPlayerCoords();

        // convert player tile coords to chunk coords
        Coords playerChunkCoords = GetChunkCoords(playerCoords.xCoord, playerCoords.yCoord);
        
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

                float[,] currentChunk = chunks[chunkCoords.ToString()];

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

    void Update() 
    {
        RenderChunks();
        //Debug.Log(chunks.Keys.Count);
    }

    void Start()
    {
        chunks = new Dictionary<string, float[,]>();
        //GenerateChunk(0, 0);
    }

}

