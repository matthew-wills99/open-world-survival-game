using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    public class Chunk
    {
        public int x {get; set;}
        public int y {get; set;}
        public int[,] chunkTiles {get; set;}

        public Chunk(int x, int y, int chunkSize)
        {
            this.x = x;
            this.y = y;
            chunkTiles = new int[chunkSize, chunkSize];

            // Initialize chunkTiles to -1 (no tile has been placed here yet)
            for(int i = 0; i < chunkSize; i++)
            {
                for(int j = 0; j < chunkSize; j++)
                {
                    chunkTiles[i, j] = -1;
                }
            }
        }
    }

    public class Biome
    {
        public BiomeEnum type {get; set;}
        public int cx {get; set;}
        public int cy {get; set;}
        public int tx {get; set;}
        public int ty {get; set;}
        public int tileIndex {get; set;}
        public int mincx;
        public int maxcx;
        public int mincy;
        public int maxcy;

        public Biome(BiomeEnum type, int cx, int cy, int tx, int ty, int tileIndex)
        {
            this.type = type;
            this.cx = cx;
            this.cy = cy;
            this.tx = tx;
            this.ty = ty;
            this.tileIndex = tileIndex;
        }

        public void setMinCX(int mincx) { this.mincx = mincx; }
        public int getMinCX() { return mincx; }

        public void setMaxCX(int maxcx) { this.maxcx = maxcx; }
        public int getMaxCX() { return maxcx; }

        public void setMinCY(int mincy) { this.mincy = mincy; }
        public int getMinCY() { return mincy; }

        public void setMaxCY(int maxcy) { this.maxcy = maxcy; }
        public int getMaxCY() { return maxcy; }


        public string GetBiomeKey()
        {
            return $"C({cx}, {cy} : T({tx}, {ty}))";
        }

        public BiomeEnum GetBiomeType()
        {
            return type;
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
