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

        public string GetBiomeKey() // what is this
        {
            return $"C({Cx}, {Cy} : T({Tx}, {Ty}))";
        }

        public BiomeEnum GetBiomeType()
        {
            return Type;
        }
    }

    public class Tree
    {
        public int Index {get; set;}
        public int Cx {get; set;}
        public int Cy {get; set;}
        public int Tx {get; set;}
        public int Ty {get; set;}

        public Tree(int index, int cx, int cy, int tx, int ty)
        {
            Index = index;
            Cx = cx;
            Cy = cy;
            Tx = tx;
            Ty = ty;
        }
    }

    public class Rock
    {
        public int Index {get; set;}
        public int Cx {get; set;}
        public int Cy {get; set;}
        public int Tx {get; set;}
        public int Ty {get; set;}

        public Rock(int index, int cx, int cy, int tx, int ty)
        {
            Index = index;
            Cx = cx;
            Cy = cy;
            Tx = tx;
            Ty = ty;
        }
    }

    public class Cactus
    {
        public int Index {get; set;}
        public int Cx {get; set;}
        public int Cy {get; set;}
        public int Tx {get; set;}
        public int Ty {get; set;}

        public Cactus(int index, int cx, int cy, int tx, int ty)
        {
            Index = index;
            Cx = cx;
            Cy = cy;
            Tx = tx;
            Ty = ty;
        }
    }

    public class Structure
    {
        public int Index {get; set;}
        public int XRad {get; set;}
        public int YUp {get; set;}
        public int YDown {get; set;}
        public int Cx {get; set;}
        public int Cy {get; set;}
        public int Tx {get; set;}
        public int Ty {get; set;}

        public Structure(int index, int xRad, int yUp, int yDown)
        {
            Index = index;
            XRad = xRad;
            YUp = yUp;
            YDown = yDown;
        }
    }

    public enum BiomeEnum
    {
        Forest,
        Desert,
        Ocean
    }

    public enum MapSize
    {
        Small,
        Medium,
        Large
    }

    public enum MenuState
    {
        Main,
        Options,
        Play,
        Quit,
        NewGame,
        LoadGame,
        ConfirmLoadGame
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

    public class WorldData
    {
        public int Seed {get; set;}
        public MapSize WorldSize {get; set;}
        public int PlayerX {get; set;}
        public int PlayerY {get; set;}
        public Dictionary<string, Chunk> AboveGroundChunks {get; set;}
        public Dictionary<string, Chunk> GroundChunks {get; set;}
        public Dictionary<string, Chunk> UnderGroundChunks {get; set;}
        public Dictionary<string, Tree> Trees {get; set;}
        public Dictionary<string, Rock> Rocks {get; set;}
        public Dictionary<string, Cactus> Cacti {get; set;}
        public List<Structure> Structures {get; set;}
    }

    // environmental object key
    public static string GetCoordinateKey(int cx, int cy, int tx, int ty)
    {
        // C(3, 1) : T(14, 9)
        return $"C({cx}, {cy}) : T({tx}, {ty})";
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
