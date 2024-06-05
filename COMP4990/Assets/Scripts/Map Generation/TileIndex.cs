using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileIndex : MonoBehaviour
{
    Dictionary<int, Tile> structureTileIndex;

    Dictionary<int, Tile> tileIndex;
    public Tile grass;
    public Tile sand;
    public Tile water;
    public Tile stone;

    public Tile d1;
    public Tile d2;
    public Tile d3;
    public Tile d4;
    public Tile d5;
    public Tile d6;
    public Tile d7;
    public Tile d8;
    public Tile d9;
    public Tile d10;
    
    void Start()
    {
        tileIndex = new Dictionary<int, Tile>();
        structureTileIndex = new Dictionary<int, Tile>();
        InitializeTileIndex();
        InitializeStructureTileIndex();
    }

    void InitializeTileIndex()
    {
        tileIndex.Add(-1, water);
        tileIndex.Add(0, grass);
        tileIndex.Add(1, sand);
        tileIndex.Add(2, water);
        tileIndex.Add(3, stone);
    }

    void InitializeStructureTileIndex()
    {
        tileIndex.Add(0, d1);
        tileIndex.Add(1, d2);
        tileIndex.Add(2, d3);
        tileIndex.Add(3, d4);
        tileIndex.Add(4, d5);
        tileIndex.Add(5, d6);
        tileIndex.Add(6, d7);
        tileIndex.Add(7, d8);
        tileIndex.Add(8, d9);
        tileIndex.Add(9, d10);
    }

    public Dictionary<int, Tile> GetStructureTileIndex()
    {
        return structureTileIndex;
    }

    public Dictionary<int, Tile> GetTileIndex()
    {
        return tileIndex;
    }
}
