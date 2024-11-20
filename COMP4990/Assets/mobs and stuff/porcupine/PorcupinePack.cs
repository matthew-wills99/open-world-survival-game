using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

using static Utils;

public class PorcupinePack: MonoBehaviour
{
    private Vector3Int origin;

    private int maxDeviationFromOrigin = 10;
    public GameObject porcupinePfb;
    public PorcupineSpawner porcupineSpawner;
    int mapSize;

    private List<GameObject> porcupines = new List<GameObject>();

    public List<GameObject> InitializePack(Vector3Int origin, int size, GameObject parent)
    {
        this.origin = origin;

        // create the porcupines
        for(int i = 0; i < size; i++)
        {
            CreateNewPorcupine(parent);
        }

        mapSize = porcupineSpawner.mapSize;

        return porcupines;
    }

    private void CreateNewPorcupine(GameObject parent)
    {
        int xOffset = Random.Range(-maxDeviationFromOrigin, maxDeviationFromOrigin + 1);
        int yOffset = Random.Range(-maxDeviationFromOrigin, maxDeviationFromOrigin + 1);

        int newX = origin.x + xOffset;
        int newY = origin.y + yOffset;
        
        if (IsWithinMapBounds(newX, newY) && porcupineSpawner.CanPlaceHere(newX, newY))
        {
            Vector3Int porcupinePos = new Vector3Int(newX, newY, origin.z);
            GameObject p = Instantiate(porcupinePfb, porcupinePos, Quaternion.identity);
            p.transform.parent = parent.transform;
            porcupines.Add(p);
            Debug.Log("Add to the list here too:");
        }
        else
        {
            // Spawn at origin if outside the map boundaries or position not allowed
            GameObject p = Instantiate(porcupinePfb, origin, Quaternion.identity);
            p.transform.parent = parent.transform;
            porcupines.Add(p);
            Debug.Log("Add to the list");
        }
    }

    private bool IsWithinMapBounds(int x, int y)
    {
        return x >= -mapSize && x <= mapSize && y >= -mapSize && y <= mapSize && porcupineSpawner.CanPlaceHere(x, y);
    }
}
