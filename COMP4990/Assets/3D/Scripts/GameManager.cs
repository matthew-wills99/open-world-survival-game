using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /*
    Game Manager will be responsible for providing all other scripts access to scriptable objects
    */

    // Singleton instance
    public static GameManager Instance {get; private set;}

    public MapGenerator mapGenerator;
    public BiomeGenerator biomeGenerator;
    public RiverGenerator riverGenerator;
    public ChunkLoader chunkLoader;

    public Dictionary<long, int[,]> chunks;
    [SerializeField, ReadOnly] public int chunkSize;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        chunkSize = mapGenerator.GetChunkSize();
    }

    private void Start()
    {
        chunks = mapGenerator.GenerateMap();
        chunkLoader.UpdateChunks();
    }
}
