using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tile : MonoBehaviour
{
    public bool grassUp;
    public bool grassRight;
    public bool grassDown;
    public bool grassLeft;

    public bool sandUp;
    public bool sandRight;
    public bool sandDown;
    public bool sandLeft;

    public bool waterUp;
    public bool waterRight;
    public bool waterDown;
    public bool waterLeft;

    public Tile[] upNeighbours;
    public Tile[] rightNeighbours;
    public Tile[] downNeighbours;
    public Tile[] leftNeighbours;

}
