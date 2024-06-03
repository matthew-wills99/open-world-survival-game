using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class StructureGenerator : MonoBehaviour
{
    public MapController mapController;
    int worldSize;
    
    public int structuresPerQuadrant = 2; // how many structures in each quadrant of the map (+, +), (-, -), (+, -), (-, +)
    public int minimumDistanceBetweenStructures = 2; // minimum distance between structures in chunks, must be at most (worldSize / 2) - 1

    /*
    quadrants are
    x < 0, y > 0
    x > 0, y > 0
    x < 0, y < 0
    x > 0, y < 0
    */

    void GenerateStructures()
    {
        
    }

    void Start()
    {
        worldSize = mapController.GetWorldSize();
        if(minimumDistanceBetweenStructures > (worldSize / 2) - 1)
        {
            minimumDistanceBetweenStructures = (worldSize / 2) - 1;
        }
    }
}
