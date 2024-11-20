using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

using static Utils;

public class PorcupineSpawner : MonoBehaviour
{
    public int spawnAreaRadius = 15;
    public int mapSize = 0;
    public GameObject porcupinePfb;
    public MapManager mapManager;
    [SerializeField] private Tilemap waterTilemap = null;
    [SerializeField] private Tilemap aboveGroundTilemap = null;

    private List<GameObject> allPorcupines = new List<GameObject>();

    private void Awake()
    {
        mapSize = mapManager.mapSizeInChunks * mapManager.chunkSize;
    }

    public void PlacePorcupine(int x, int y)
    {
        Instantiate(porcupinePfb, new Vector3Int(x, y, 0), Quaternion.identity);
    }

    public List<CPorcupine> GetPorcupinesListForWorldGen()
    {
        Debug.Log("Here");
        List<CPorcupine> returnList = new List<CPorcupine>();
        foreach(GameObject p in allPorcupines)
        {
            if(p != null)
            {
                Debug.Log("Deep");
                CPorcupine cPorcupine = new CPorcupine(Mathf.FloorToInt(p.transform.position.x), Mathf.FloorToInt(p.transform.position.y));
                returnList.Add(cPorcupine);
            }
        }
        return returnList;
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

        List<GameObject> porcupinesFromPack = newPack.InitializePack(randomPos, porcupinesPerPack, parent);
        foreach(GameObject p in porcupinesFromPack)
        {
            allPorcupines.Add(p);
        }
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
