using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    public class Chunk
    {
        public int X {get; set;}
        public int Y {get; set;}
        public int[,] ChunkTiles {get; set;}

        public Chunk(int x, int y, int chunkSize)
        {
            X = x;
            Y = y;
            ChunkTiles = new int[chunkSize, chunkSize];

            // Initialize chunkTiles to -1 (no tile has been placed here yet)
            for(int i = 0; i < chunkSize; i++)
            {
                for(int j = 0; j < chunkSize; j++)
                {
                    ChunkTiles[i, j] = -1;
                }
            }
        }
    }

    public class Biome
    {
        public BiomeEnum Type {get; set;}
        public int Cx {get; set;}
        public int Cy {get; set;}
        public int Tx {get; set;}
        public int Ty {get; set;}
        public int TileIndex {get; set;}

        public Biome(BiomeEnum type, int cx, int cy, int tx, int ty, int tileIndex)
        {
            Type = type;
            Cx = cx;
            Cy = cy;
            Tx = tx;
            Ty = ty;
            TileIndex = tileIndex;
        }

        public string GetBiomeKey()
        {
            return $"C({Cx}, {Cy} : T({Tx}, {Ty}))";
        }

        public BiomeEnum GetBiomeType()
        {
            return Type;
        }
    }

    public class Structure
    {
        public Grid StructGrid {get; set;}
        public int XRad {get; set;}
        public int YUp {get; set;}
        public int YDown {get; set;}
        public int Cx {get; set;}
        public int Cy {get; set;}
        public int Tx {get; set;}
        public int Ty {get; set;}

        public Structure(Grid structGrid, int xRad, int yUp, int yDown)
        {
            StructGrid = structGrid;
            XRad = xRad;
            YUp = yUp;
            YDown = yDown;
        }
    }

    public enum BiomeEnum
    {
        Forest,
        Desert,
        Plains
    }

    public enum MapSize
    {
        Small,
        Medium,
        Large
    }

    public enum BBTKey
    {
        Null,
        BlendLeft,
        BlendRight,
        BlendUp,
        BlendDown,
        BlendUpLeft,
        BlendUpRight,
        BlendDownLeft,
        BlendDownRight
    }

    // Return a chunk key from the x and y values of the desired chunk
    public static string GetChunkKey(int x, int y)
    {
        return $"C({x}, {y})";
    }

    public static Vector3Int ChunkToWorldPos(int cx, int cy, int tx, int ty, int chunkSize)
    {
        return new Vector3Int(cx * chunkSize + tx , cy * chunkSize + ty, 0);
    }

    public static (int, int, int, int) WorldToChunkPos(int wx, int wy, int chunkSize)
    {
        // cx, cy, tx, ty
        int cx = (int)Math.Floor((double)wx / chunkSize);
        int cy = (int)Math.Floor((double)wy / chunkSize);

        int tx = (wx % chunkSize + chunkSize) % chunkSize;
        int ty = (wy % chunkSize + chunkSize) % chunkSize;
        
        return (cx, cy, tx, ty);
    }

    public static bool TileInChunk(int[,] chunkTiles, int tile)
    {
        foreach(int t in chunkTiles)
        {
            if(t == tile)
            {
                return true;
            }
        }
        return false;
    }

    public static double GetDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Abs(Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)));
    }
}
