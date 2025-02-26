using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Utils;

public class ChunkLoader : MonoBehaviour
{
    public Transform target;
    public int renderDistance = 3;
    public int maxChunkLoadActionsPerFrame = 5;

    private int chunkSize;
    private Vector3Int targetChunk;
    private HashSet<long> loadedChunks = new HashSet<long>();

    private bool isLoadingAndUnloadingChunks = false;

    private Transform mapParentTransform;
    private Dictionary<long, GameObject> chunkParentObjects = new Dictionary<long, GameObject>();

    private void Awake()
    {
        chunkSize = GameManager.Instance.chunkSize;

        // Empty gameobject to act as a folder for all chunks (Map Parent > Chunk Parent > All blocks)
        mapParentTransform = new GameObject("Map Parent").transform;
    }

    private void Update()
    {
        Vector3Int newTargetChunk = GetCurrentTargetChunk();

        if(newTargetChunk != targetChunk)
        {
            targetChunk = newTargetChunk;
            UpdateChunks();
        }
    }

    private Vector3Int GetCurrentTargetChunk()
    {
        return GetChunkAt(
            Mathf.FloorToInt(target.position.x),
            Mathf.FloorToInt(target.position.z),
            chunkSize
        );
    }

    public void UpdateChunks()
    {
        if(!isLoadingAndUnloadingChunks)
        {
            StartCoroutine(LoadAndUnloadChunks());
        }
    }

    private IEnumerator LoadAndUnloadChunks()
    {
        isLoadingAndUnloadingChunks = true;
        
        while(true)
        {
            yield return StartCoroutine(LoadChunksCoroutine());
            yield return StartCoroutine(UnloadChunksCoroutine());
        }
    }

    private IEnumerator LoadChunksCoroutine()
    {
        int chunksLoaded = 0;

        List<KeyValuePair<long, float>> chunkDistances = new List<KeyValuePair<long, float>>();

        for (int cx = targetChunk.x - renderDistance; cx <= targetChunk.x + renderDistance; cx++)
        {
            for (int cy = targetChunk.y - renderDistance; cy <= targetChunk.y + renderDistance; cy++)
            {
                long chunkKey = GetChunkKey(cx, cy);
                /*
                make sure we are not trying to load a chunk that does not exist
                e.g. standing on the border of the map trying to render a chunk outside the map
                */
                if(!GameManager.Instance.chunks.ContainsKey(chunkKey))
                {
                    continue;
                }

                float distance = Vector2.Distance(new Vector2(cx, cy), new Vector2(targetChunk.x, targetChunk.y));
                chunkDistances.Add(new KeyValuePair<long, float>(chunkKey, distance));
            }
        }

        // sort by closest chunks first
        chunkDistances.Sort((a, b) => a.Value.CompareTo(b.Value));

        foreach(var chunkDistance in chunkDistances)
        {
            long chunkKey = chunkDistance.Key;

            // load unloaded chunk
            if (!loadedChunks.Contains(chunkKey))
            {
                GetChunkCoordsFromKey(chunkKey, out int cx, out int cy);
                LoadChunk(cx, cy);
                loadedChunks.Add(chunkKey);
            }

            // wait until next frame to continue loading chunks if enough chunks have already been loaded.
            chunksLoaded++;
            if(chunksLoaded % maxChunkLoadActionsPerFrame == 0)
            {
                yield return null; // wait for next frame
            }
        }
    }

    private IEnumerator UnloadChunksCoroutine()
    {
        int chunksUnloaded = 0;

        List<long> chunksToUnload = new List<long>();

        foreach (long chunkKey in loadedChunks)
        {
            int cx, cy;
            GetChunkCoordsFromKey(chunkKey, out cx, out cy);

            if (Mathf.Abs(cx - targetChunk.x) > renderDistance || Mathf.Abs(cy - targetChunk.y) > renderDistance)
            {
                chunksToUnload.Add(chunkKey); // Mark for removal
            }
        }

        // Unload the chunks outside of the foreach loop
        foreach (long chunkKey in chunksToUnload)
        {
            UnloadChunk(chunkKey);

            chunksUnloaded++;

            if (chunksUnloaded % maxChunkLoadActionsPerFrame == 0)
            {
                yield return null; // wait for next frame
            }
        }
    }

    private void LoadChunk(int cx, int cy) 
    {
        long chunkKey = GetChunkKey(cx, cy);

        if(chunkParentObjects.ContainsKey(chunkKey))
        {
            return;
        }

        // Empty gameobject that will be used as a 'folder' to hold all the block gameobjects within a chunk (looks nice in the hierarchy)
        GameObject chunkParentObject = new GameObject($"Chunk: ({cx}, {cy})");
        chunkParentObject.transform.SetParent(mapParentTransform);
        chunkParentObjects.Add(chunkKey, chunkParentObject);

        int[,] currentChunk = GameManager.Instance.chunks[chunkKey];
        for(int x = 0; x < chunkSize; x++)
        {
            for(int y = 0; y < chunkSize; y++)
            {
                BlockManager.Instance.CreateBlock(ChunkToWorldPos(cx, cy, x, y, 0, chunkSize), currentChunk[x, y], chunkParentObject.transform);
            }
        }
    }

    private void UnloadChunk(long chunkKey)
    {
        if(loadedChunks.Contains(chunkKey))
        {
            loadedChunks.Remove(chunkKey);
        }

        if(chunkParentObjects.ContainsKey(chunkKey))
        {
            Destroy(chunkParentObjects[chunkKey]);
            chunkParentObjects.Remove(chunkKey);
        }

        BlockManager.Instance.DestroyBlocksInChunk(chunkKey);
    }
}
