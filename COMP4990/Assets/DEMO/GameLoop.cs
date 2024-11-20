using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    public static GameLoop Instance { get; private set;}

    public PorcupineSpawner porcupineSpawner;
    public GameObject parentPorcupineObj;

    /*
    Mobs
    Mob cap: 20
    spawn porcupines in groups around the same area
        groups will be 2 to 5 porcupines

    */

    public int porcupineCap = 20;
    private int porcupinesPlaced = 0;

    public int porcupineMin = 2;
    public int porcupineMax = 5;

    // 120 seconds between checking and spawning mobs when they die
    public float spawnMobsInterval = 120f;
    private float spawnMobsAt = 0f;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SpawnMobs();
    }

    private void Update()
    {
        if(Time.time > spawnMobsAt)
        {
            SpawnMobs();
            spawnMobsAt = Time.time + spawnMobsInterval;
        }
    }

    private void SpawnMobs()
    {
        /*
        Spawn a random number of porcupines between 2 and 5+1, only if mobcap - mobs >= 5
        otherwise spawn mobcap - mob porcupines in the group
        */

        while(porcupineCap - porcupinesPlaced >= 5)
        {
            int mobsToPlace = Random.Range(porcupineMin, porcupineMax + 1);
            porcupineSpawner.SpawnPorcupinePack(mobsToPlace, parentPorcupineObj);
            porcupinesPlaced += mobsToPlace;
        }
        if(porcupineCap - porcupinesPlaced > 0 && porcupineCap - porcupinesPlaced < 5)
        {
            porcupineSpawner.SpawnPorcupinePack(porcupineCap - porcupinesPlaced, parentPorcupineObj);
            porcupinesPlaced += porcupineCap - porcupinesPlaced;
        }
    }

    public void OnPorcupineDeath()
    {
        Debug.Log("RIP lil bro");
        porcupinesPlaced--;
    }
}
