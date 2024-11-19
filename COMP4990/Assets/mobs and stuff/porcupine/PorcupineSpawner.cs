using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PorcupineSpawner : MonoBehaviour
{
    public int packs = 3;
    public int porcupinesPerPack = 5;
    public int spawnAreaRadius = 15;

    public int mapSize = 0;

    public GameObject porcupinePfb;

    public MapManager mapManager;
    [SerializeField] private Tilemap waterTilemap = null;
    [SerializeField] private Tilemap aboveGroundTilemap = null;

    private void Start()
    {
        mapSize = mapManager.mapSizeInChunks * mapManager.chunkSize;

        SpawnPorcupinePacks();
    }

    private void SpawnPorcupinePacks()
    {
        for(int i = 0; i < packs; i++)
        {
            Vector3Int randomPos = GetRandomSpawnPosition();
            Debug.Log("Here");
            GameObject packObj = new GameObject($"PorcupinePack_{i + 1}");
            packObj.transform.position = randomPos;

            PorcupinePack newPack = packObj.AddComponent<PorcupinePack>();
            newPack.porcupinePfb = porcupinePfb;
            newPack.porcupineSpawner = this;
            newPack.InitializePack(randomPos, porcupinesPerPack);
        }
    }

    private Vector3Int GetRandomSpawnPosition()
    {
        // i hope this does not crash my computer
        while(true)
        {
            int x = Random.Range(-mapSize / 2, mapSize / 2+ 1);
            int y = Random.Range(-mapSize / 2, mapSize / 2 + 1);

            if(CanPlaceHere(x, y))
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
}
