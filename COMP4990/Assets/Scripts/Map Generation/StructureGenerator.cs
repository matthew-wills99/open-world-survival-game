using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class StructureGenerator : MonoBehaviour
{
    public MapController mapController;
    public TileIndex tileIndex;
    int worldSize;
    
    public int structuresPerQuadrant = 2; // how many structures in each quadrant of the map (+, +), (-, -), (+, -), (-, +)
    public int minimumDistanceBetweenStructures = 2; // minimum distance between structures in chunks, must be at most (worldSize / 2) - 1

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    // tile number linked to list of directions linked to list of acceptable neighbours
    public List<KeyValuePair<int, List<KeyValuePair<Direction, List<int>>>>> tileNeighbours;

    /*
    LIST
    {
        (0, List of directions)
            (up, list of valid tiles)
                (0, 1, 2) <- tiles that can be placed above this tile (0)
            (down, list of valid tiles)
                (0) <- tiles that can be placed below this tile (0)
            (right, list of valid tiles)
                (0, 1, 2) <- tiles that can be placed right of this tile (0)
            (left, list of valid tiles)
                (0, 1) <- tiles that can be placed left of this tile (0)
        etc...
    }
    */

    /*
    quadrants are
    x < 0, y > 0
    x > 0, y > 0
    x < 0, y < 0
    x > 0, y < 0
    */

    void GenerateStructures()
    {
        return;
    }

    // Set all the neighbours for each tile in the structure list
    void InitializeNeighbours()
    {
        tileNeighbours = new List<KeyValuePair<int, List<KeyValuePair<Direction, List<int>>>>>
        {
            // add new tile to the tileNeighbours list
            new KeyValuePair<int, List<KeyValuePair<Direction, List<int>>>>(
                0, // tile number (as per TileIndex.GetStructureTileIndex())
                new List<KeyValuePair<Direction, List<int>>>{ // List of directions, each with a list of valid neighbour tiles for that direction
                    new KeyValuePair<Direction, List<int>>( // specific direction
                        Direction.Up, new List<int>{ // up direction
                            1, // valid tiles in this direction
                            7,
                            8,
                            9,
                            10
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Down, new List<int>{ // down direction
                            6,
                            7
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Left, new List<int>{ // left direction
                            2,
                            4,
                            5,
                            8,
                            10
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Right, new List<int>{ // right direction
                            1,
                            2,
                            4,
                            8
                        }
                    )
                }
            ),

            new KeyValuePair<int, List<KeyValuePair<Direction, List<int>>>>(
                1,
                new List<KeyValuePair<Direction, List<int>>>{
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Up, new List<int>{ // up direction
                            1,
                            7,
                            8,
                            9,
                            10
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Down, new List<int>{ // down direction
                            6,
                            7
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Left, new List<int>{ // left direction
                            2,
                            4,
                            5,
                            8
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Right, new List<int>{ // right direction
                            1,
                            2,
                            4,
                            8
                        }
                    )
                }
            ),

            new KeyValuePair<int, List<KeyValuePair<Direction, List<int>>>>(
                2,
                new List<KeyValuePair<Direction, List<int>>>{
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Up, new List<int>{ // up direction
                            1,
                            7,
                            8,
                            9,
                            10
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Down, new List<int>{ // down direction
                            5,
                            8
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Left, new List<int>{ // left direction
                            0,
                            1,
                            3,
                            7
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Right, new List<int>{ // right direction
                            0,
                            3,
                            6,
                            7,
                            10
                        }
                    )
                }
            ),

            new KeyValuePair<int, List<KeyValuePair<Direction, List<int>>>>(
                3,
                new List<KeyValuePair<Direction, List<int>>>{
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Up, new List<int>{ // up direction
                            1,
                            7,
                            8,
                            9
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Down, new List<int>{ // down direction
                            6,
                            7
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Left, new List<int>{ // left direction
                            2,
                            4,
                            5,
                            8
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Right, new List<int>{ // right direction
                            1,
                            2,
                            4,
                            8,
                            9
                        }
                    )
                }
            ),

            new KeyValuePair<int, List<KeyValuePair<Direction, List<int>>>>(
                4,
                new List<KeyValuePair<Direction, List<int>>>{
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Up, new List<int>{ // up direction
                            1,
                            7,
                            8,
                            9
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Down, new List<int>{ // down direction
                            5,
                            8
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Left, new List<int>{ // left direction
                            0,
                            1,
                            3,
                            7,
                            9
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Right, new List<int>{ // right direction
                            0,
                            3,
                            6,
                            7
                        }
                    )
                }
            ),

            new KeyValuePair<int, List<KeyValuePair<Direction, List<int>>>>(
                5,
                new List<KeyValuePair<Direction, List<int>>>{
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Up, new List<int>{ // up direction
                            0,
                            2,
                            4,
                            5
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Down, new List<int>{ // down direction
                            5,
                            7,
                            8
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Left, new List<int>{ // left direction
                            2,
                            6,
                            8,
                            10
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Right, new List<int>{ // right direction
                            1,
                            2,
                            4,
                            8
                        }
                    )
                }
            ),

            new KeyValuePair<int, List<KeyValuePair<Direction, List<int>>>>(
                6,
                new List<KeyValuePair<Direction, List<int>>>{
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Up, new List<int>{ // up direction
                            0,
                            2,
                            3,
                            6
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Down, new List<int>{ // down direction
                            6,
                            7,
                            8
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Left, new List<int>{ // left direction
                            2,
                            4,
                            5,
                            8
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Right, new List<int>{ // right direction
                            0,
                            5,
                            7,
                            10
                        }
                    )
                }
            ),

            new KeyValuePair<int, List<KeyValuePair<Direction, List<int>>>>(
                7,
                new List<KeyValuePair<Direction, List<int>>>{
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Up, new List<int>{ // up direction
                            0,
                            2,
                            3,
                            4,
                            6
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Down, new List<int>{ // down direction
                            0,
                            1,
                            2,
                            3,
                            4,
                            9,
                            10
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Left, new List<int>{ // left direction
                            2,
                            4,
                            5,
                            6,
                            8,
                            10
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Right, new List<int>{ // right direction
                            2,
                            4,
                            8,
                            9
                        }
                    )
                }
            ),

            new KeyValuePair<int, List<KeyValuePair<Direction, List<int>>>>(
                8,
                new List<KeyValuePair<Direction, List<int>>>{
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Up, new List<int>{ // up direction
                            0,
                            2,
                            3,
                            4,
                            5
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Down, new List<int>{ // down direction
                            0,
                            1,
                            2,
                            3,
                            4,
                            9,
                            10
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Left, new List<int>{ // left direction
                            0,
                            3,
                            7,
                            9
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Right, new List<int>{ // right direction
                            0,
                            3,
                            5,
                            6,
                            7,
                            10
                        }
                    )
                }
            ),

            new KeyValuePair<int, List<KeyValuePair<Direction, List<int>>>>(
                9,
                new List<KeyValuePair<Direction, List<int>>>{
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Up, new List<int>{ // up direction
                            1,
                            7,
                            8,
                            9,
                            10
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Down, new List<int>{ // down direction
                            0,
                            1,
                            2,
                            3,
                            4,
                            10
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Left, new List<int>{ // left direction
                            3,
                            7,
                            9
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Right, new List<int>{ // right direction
                            4,
                            8,
                            9
                        }
                    )
                }
            ),

            new KeyValuePair<int, List<KeyValuePair<Direction, List<int>>>>(
                10,
                new List<KeyValuePair<Direction, List<int>>>{
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Up, new List<int>{ // up direction
                            1,
                            7,
                            8,
                            9,
                            10
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Down, new List<int>{ // down direction
                            0,
                            1,
                            2,
                            3,
                            4,
                            9,
                            10
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Left, new List<int>{ // left direction
                            2,
                            4,
                            5,
                            6,
                            8,
                            10
                        }
                    ),
                    new KeyValuePair<Direction, List<int>>(
                        Direction.Right, new List<int>{ // right direction
                            0,
                            3,
                            5,
                            6,
                            7,
                            10
                        }
                    )
                }
            ),
        };
    }

    public List<KeyValuePair<int, List<KeyValuePair<Direction, List<int>>>>> GetNeighbours()
    {
        return tileNeighbours;
    }

    void Awake()
    {
        InitializeNeighbours();
        worldSize = mapController.GetWorldSize();
        if(minimumDistanceBetweenStructures > (worldSize / 2) - 1)
        {
            minimumDistanceBetweenStructures = (worldSize / 2) - 1;
        }
    }
}
