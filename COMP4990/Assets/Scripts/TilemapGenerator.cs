using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGenerator : MonoBehaviour
{
    public enum Grid
    {
        FLOOR,
        EMPTY
    }

    public Grid[,] gridHandler;

    public Tilemap groundMap;
    public Tilemap pathMap;
    public Tilemap structureMap;
    public Tilemap wallMap;

    public int MapWidth = 16;
    public int MapHeight = 16;

    void Start()
    {
        InitGrid();
    }

    void InitGrid()
    {
        gridHandler = new Grid[MapWidth, MapHeight];
        for(int x = 0; x < gridHandler.GetLength(0); x++)
        {
            for(int y = 0; y < gridHandler.GetLength(1); y++)
            {
                gridHandler[x, y] = Grid.EMPTY;
            }
        }
    }
}
