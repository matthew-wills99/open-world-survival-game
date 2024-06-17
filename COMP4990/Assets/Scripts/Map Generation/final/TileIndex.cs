using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileIndex : MonoBehaviour
{
    Dictionary<int, Tile> tileIndex;
    List<int> grassTiles;
    List<int> grassWithFlowersTiles;

    public Tile grass;
    public Tile grass2;
    public Tile grass3;
    public Tile grass4;
    public Tile grass5;
    public Tile grass6;
    public Tile grass7;
    public Tile grass8;
    public Tile grass9;
    public Tile grass10;
    public Tile grass11;
    public Tile grass12;
    public Tile grass13;
    public Tile grass14;
    public Tile grass15;
    public Tile grass16;
    public Tile grass17;
    public Tile grassWithFlowers;
    public Tile grassWithFlowers2;
    public Tile grassWithFlowers3;
    public Tile grassWithFlowers4;
    public Tile grassWithFlowers5;
    public Tile grassWithFlowers6;
    public Tile grassWithFlowers7;
    public Tile grassWithFlowers8;
    public Tile grassWithFlowers9;
    public Tile grassWithFlowers10;
    public Tile grassWithFlowers11;
    public Tile grassWithFlowers12;
    public Tile grassWithFlowers13;
    public Tile grassWithFlowers14;
    public Tile grassWithFlowers15;
    public Tile stone;
    public Tile sand;
    public Tile water;

    void Awake()
    {
        tileIndex = new Dictionary<int, Tile>{
            { 0, grass},
            { 1, grass2},
            { 2, grass3},
            { 3, grass4},
            { 4, grass5},
            { 5, grass6},
            { 6, grass7},
            { 7, grass8},
            { 8, grass9},
            { 9, grass10},
            { 10, grass11},
            { 11, grass12},
            { 12, grass13},
            { 13, grass14},
            { 14, grass15},
            { 15, grass16},
            { 16, grass17},
            { 17, grassWithFlowers},
            { 18, grassWithFlowers2},
            { 19, grassWithFlowers3},
            { 20, grassWithFlowers4},
            { 21, grassWithFlowers5},
            { 22, grassWithFlowers6},
            { 23, grassWithFlowers7},
            { 24, grassWithFlowers8},
            { 25, grassWithFlowers9},
            { 26, grassWithFlowers10},
            { 27, grassWithFlowers11},
            { 28, grassWithFlowers12},
            { 29, grassWithFlowers13},
            { 30, grassWithFlowers14},
            { 31, grassWithFlowers15},
            { 32, stone},
            { 33, sand},
            { 34, water},
        };

        grassTiles = new List<int>{
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16
        };

        grassWithFlowersTiles = new List<int>{
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31
        };
    }

    public List<int> GetGrassTiles()
    {
        return grassTiles;
    }

    public List<int> GetGrassWithFlowersTiles()
    {
        return grassWithFlowersTiles;
    }

    public Tile GetTile(int index)
    {
        return tileIndex[index];
    }
}
