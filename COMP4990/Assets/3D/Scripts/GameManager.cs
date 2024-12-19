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
        DontDestroyOnLoad(gameObject);

        chunks = mapGenerator.GenerateMap();
        chunkSize = mapGenerator.GetChunkSize();
    }

    private void Start()
    {
        chunkLoader.UpdateChunks();
    }
}
