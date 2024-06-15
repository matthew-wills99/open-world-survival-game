using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileIndex : MonoBehaviour
{
    Dictionary<int, Tile> structureTileIndex;

    Dictionary<int, Tile> tileIndex;
    public Tile farGrass;
    public Tile grass;
    public Tile sand;
    public Tile water;
    public Tile stone;
    public Tile purpleThing;

    public Tile path1;
    public Tile path2;
    public Tile pathCracked1;
    public Tile pathCracked2;
    public Tile pathLeft;
    public Tile pathLeftCracked;
    public Tile pathRight;
    public Tile pathRightCracked;
    public Tile pathUp;
    public Tile pathUpCracked;
    public Tile pathDown;
    public Tile pathDownCracked;
    public Tile structTopLeft;
    public Tile structTopMiddle;
    public Tile structTopRight;
    public Tile structMiddleLeft;
    public Tile structMiddle;
    public Tile structMiddleRight;
    public Tile structBottomLeft;
    public Tile structBottomMiddle;
    public Tile structBottomRight;
    public Tile structGrass1;
    public Tile structGrass2;
    public Tile structGrass3;
    public Tile structGrass4;
    
    void Awake()
    {
        tileIndex = new Dictionary<int, Tile>();
        structureTileIndex = new Dictionary<int, Tile>();
        InitializeTileIndex();
        InitializeStructureTileIndex();
    }

    void InitializeTileIndex()
    {
        tileIndex.Add(-1, null);
        tileIndex.Add(0, grass);
        tileIndex.Add(1, sand);
        tileIndex.Add(2, water);
        tileIndex.Add(3, stone);
        tileIndex.Add(4, path1);
        tileIndex.Add(5, path2);
        tileIndex.Add(6, pathCracked1);
        tileIndex.Add(7, pathCracked2);
        tileIndex.Add(8, pathLeft);
        tileIndex.Add(9, pathLeftCracked);
        tileIndex.Add(10, pathRight);
        tileIndex.Add(11, pathRightCracked);
        tileIndex.Add(12, pathUp);
        tileIndex.Add(13, pathUpCracked);
        tileIndex.Add(14, pathDown);
        tileIndex.Add(15, pathDownCracked);
        tileIndex.Add(16, structTopLeft);
        tileIndex.Add(17, structTopMiddle);
        tileIndex.Add(18, structTopRight);
        tileIndex.Add(19, structMiddleLeft);
        tileIndex.Add(20, structMiddle);
        tileIndex.Add(21, structMiddleRight);
        tileIndex.Add(22, structBottomLeft);
        tileIndex.Add(23, structBottomMiddle);
        tileIndex.Add(24, structBottomRight);
        tileIndex.Add(25, structGrass1);
        tileIndex.Add(26, structGrass2);
        tileIndex.Add(27, structGrass3);
        tileIndex.Add(28, structGrass4);
        // start next at 100
        tileIndex.Add(100, purpleThing);
        tileIndex.Add(101, farGrass);
    }

    void InitializeStructureTileIndex()
    {
        structureTileIndex.Add(0, path1);
        structureTileIndex.Add(1, path2);
        structureTileIndex.Add(2, pathCracked1);
        structureTileIndex.Add(3, pathCracked2);
        structureTileIndex.Add(4, pathLeft);
        structureTileIndex.Add(5, pathLeftCracked);
        structureTileIndex.Add(6, pathRight);
        structureTileIndex.Add(7, pathRightCracked);
        structureTileIndex.Add(8, pathUp);
        structureTileIndex.Add(9, pathUpCracked);
        structureTileIndex.Add(10, pathDown);
        structureTileIndex.Add(11, pathDownCracked);
        structureTileIndex.Add(12, structTopLeft);
        structureTileIndex.Add(13, structTopMiddle);
        structureTileIndex.Add(14, structTopRight);
        structureTileIndex.Add(15, structMiddleLeft);
        structureTileIndex.Add(16, structMiddle);
        structureTileIndex.Add(17, structMiddleRight);
        structureTileIndex.Add(18, structBottomLeft);
        structureTileIndex.Add(19, structBottomMiddle);
        structureTileIndex.Add(20, structBottomRight);
        structureTileIndex.Add(21, structGrass1);
        structureTileIndex.Add(22, structGrass2);
        structureTileIndex.Add(23, structGrass3);
        structureTileIndex.Add(24, structGrass4);
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
