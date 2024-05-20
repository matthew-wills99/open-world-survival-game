using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileIndex : MonoBehaviour
{
    Dictionary<int, Tile> tileIndex;
    public Tile grass;
    public Tile sand;
    public Tile water;
    
    void Start()
    {
        tileIndex = new Dictionary<int, Tile>();
        tileIndex.Add(-1, water);
        tileIndex.Add(0, grass);
        tileIndex.Add(1, sand);
        tileIndex.Add(2, water);
    }

    public Dictionary<int, Tile> GetTileIndex()
    {
        return tileIndex;
    }
}
