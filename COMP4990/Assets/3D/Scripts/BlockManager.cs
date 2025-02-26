using System.Collections.Generic;
using UnityEngine;

using static Utils;

public class BlockManager : MonoBehaviour
{
    // Singleton instance
    public static BlockManager Instance {get; private set;}

    public BlockIndex blockIndex;

    private Dictionary<Vector2Int, GameObject> blockObjects = new Dictionary<Vector2Int, GameObject>();

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void CreateBlock(Vector3Int pos, int index, Transform parent = null)
    {
        CreateBlock(pos, blockIndex.GetBlockByID(index), parent);
    }

    public void CreateBlock(Vector3Int pos, BlockScriptable blockScriptable, Transform parent = null)
    {
        /*
        *FOR FUTURE REFERENCE: (POS.X, POS.Y) ARE THE COORDINATES, POS.Z IS THE LAYER

        It is important to remember that when converting from 2d to 3d positions, the y of 2d becomes the z of 3d.

        The position of the block must be in 3D, this means that y and z are flipped, as y is vertical in 2D (player POV).

        We need to consider some things when creating blocks, we do not care about faces that cannot be seen.
        */        
        Block block = new Block(new Vector3(pos.x, pos.z, pos.y), blockScriptable);
        GameObject blockObject = new GameObject($"B_{blockScriptable.name}: ({pos.x}, {pos.y})");

        if(parent)
        {
            blockObject.transform.SetParent(parent);
        }

        blockObject.AddComponent<MeshFilter>().mesh = block.GetMesh();
        blockObject.AddComponent<MeshRenderer>().material.mainTexture = block.GetTextureAtlas();
        blockObjects.Add(new Vector2Int(pos.x, pos.y), blockObject);
    }

    public void DestroyBlocksInChunk(long chunkKey)
    {
        int x, y;
        int chunkSize = MapGenerator.chunkSize;

        GetChunkCoordsFromKey(chunkKey, out int cx, out int cy);

        for(int bx = 0; bx < chunkSize; bx++)
        {
            for(int by = 0; by < chunkSize; by++)
            {
                x = (cx * chunkSize) + bx;
                y = (cy * chunkSize) + by;

                Vector2Int blockPos = new Vector2Int(x, y);
                if(blockObjects.ContainsKey(blockPos))
                {
                    Destroy(blockObjects[blockPos]);
                    blockObjects.Remove(blockPos);
                }
            }
        }
    }
}