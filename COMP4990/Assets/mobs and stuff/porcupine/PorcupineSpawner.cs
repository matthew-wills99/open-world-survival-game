using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PorcupineSpawner : MonoBehaviour
{
    public int spawnAreaRadius = 15;
    public int mapSize = 0;
    public GameObject porcupinePfb;
    public MapManager mapManager;
    [SerializeField] private Tilemap waterTilemap = null;
    [SerializeField] private Tilemap aboveGroundTilemap = null;

    private GameObject lastSpawnedPack;

    private void Awake()
    {
        mapSize = mapManager.mapSizeInChunks * mapManager.chunkSize;
    }

    public void SpawnPorcupinePack(int porcupinesPerPack, GameObject parent)
    {
        Vector3Int randomPos = GetRandomSpawnPosition();
        GameObject packObj = new GameObject("PorcupinePack");
        packObj.transform.parent = parent.transform;
        packObj.transform.position = randomPos;

        PorcupinePack newPack = packObj.AddComponent<PorcupinePack>();
        newPack.porcupinePfb = porcupinePfb;
        newPack.porcupineSpawner = this;
        newPack.InitializePack(randomPos, porcupinesPerPack, parent);

        lastSpawnedPack = packObj;
    }

    public GameObject GetLastSpawnedPack()
    {
        return lastSpawnedPack;
    }

    private Vector3Int GetRandomSpawnPosition()
    {
        // i hope this does not crash my computer
        while(true)
        {
            int x = Random.Range(-mapSize / 2, mapSize / 2+ 1);
            int y = Random.Range(-mapSize / 2, mapSize / 2 + 1);

            if(CanPlaceHere(x, y) && IsWithinMapBounds(x, y))
            {
                return new Vector3Int(x, y, 0);
            }
        }
    }

    public bool CanPlaceHere(int x, int y)
    {
        Debug.Log($"Trying {x}, {y}");
        // trying to place in water (will add bridges)
        if(waterTilemap.GetTile(new Vector3Int(x, y, 0)) != null)
        {
            Debug.Log("Water");
            return false;
        }
        // make sure no other tile already exists
        if(aboveGroundTilemap.GetTile(new Vector3Int(x, y, 0)) == null)
        {
            return true;
        }
        Debug.Log("above ground chunks");
        return false;
    }

    private bool IsWithinMapBounds(int x, int y)
    {
        return x >= -mapSize && x <= mapSize && y >= -mapSize && y <= mapSize;
    }
}
