using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PorcupinePack: MonoBehaviour
{
    private Vector3Int origin;
    private int size;
    private List<GameObject> porcupines;

    private int maxDeviationFromOrigin = 10;

    public GameObject porcupinePfb;

    public PorcupineSpawner porcupineSpawner;

    int mapSize;

    public void InitializePack(Vector3Int origin, int size)
    {
        this.origin = origin;
        this.size = size;

        porcupines = new List<GameObject>();

        // create the porcupines
        for(int i = 0; i < size; i++)
        {
            CreateNewPorcupine();
        }

        mapSize = porcupineSpawner.mapSize;
    }

    private void CreateNewPorcupine()
    {
        int xOffset = Random.Range(-maxDeviationFromOrigin, maxDeviationFromOrigin + 1);
        int yOffset = Random.Range(-maxDeviationFromOrigin, maxDeviationFromOrigin + 1);

        int newX = origin.x + xOffset;
        int newY = origin.y + yOffset;
        
        if (IsWithinMapBounds(newX, newY) && porcupineSpawner.CanPlaceHere(newX, newY))
        {
            Vector3Int porcupinePos = new Vector3Int(newX, newY, origin.z);
            GameObject porcupine = Instantiate(porcupinePfb, porcupinePos, Quaternion.identity);
            porcupines.Add(porcupine);
        }
        else
        {
            // Spawn at origin if outside the map boundaries or position not allowed
            GameObject porcupine = Instantiate(porcupinePfb, origin, Quaternion.identity);
            porcupines.Add(porcupine);
        }
    }

    private bool IsWithinMapBounds(int x, int y)
    {
        return x >= -mapSize && x <= mapSize && y >= -mapSize && y <= mapSize;
    }
}
