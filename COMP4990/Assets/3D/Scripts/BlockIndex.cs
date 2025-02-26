using System.Collections.Generic;
using UnityEngine;

using static Utils;

[CreateAssetMenu(fileName = "BlockIndex", menuName = "Block Index")]
public class BlockIndex : ScriptableObject
{
    public List<BlockEntry> blocks = new List<BlockEntry>();

    private Dictionary<int, BlockScriptable> blockLookupByID;
    private Dictionary<string, BlockScriptable> blockLookupByName;

    // Initialize the lookup dictionary
    public void Initialize()
    {
        blockLookupByID = new Dictionary<int, BlockScriptable>();
        blockLookupByName = new Dictionary<string, BlockScriptable>();

        foreach (var entry in blocks)
        {
            entry.blockScriptable.SetID(entry.blockID);

            if (!blockLookupByID.ContainsKey(entry.blockID))
            {
                blockLookupByID.Add(entry.blockID, entry.blockScriptable);
            }
            else
            {
                Debug.LogWarning($"Duplicate block ID {entry.blockID} found in the Block Index!");
            }

            if (!blockLookupByName.ContainsKey(entry.blockName))
            {
                blockLookupByName.Add(entry.blockName.ToLower(), entry.blockScriptable); // Store name as lowercase for case-insensitive lookup
            }
            else
            {
                Debug.LogWarning($"Duplicate block name '{entry.blockName}' found!");
            }
        }
    }

    // Get BlockScriptable by ID
    public BlockScriptable GetBlockByID(int id)
    {
        if (blockLookupByID == null)
            Initialize();

        if (blockLookupByID.TryGetValue(id, out var block))
        {
            return block;
        }

        Debug.LogError($"Block ID {id} not found!");
        return null;
    }

    public BlockScriptable GetBlockByName(string name)
    {
        if (blockLookupByName == null)
            Initialize();

        if (blockLookupByName.TryGetValue(name.ToLower(), out var block))
        {
            return block;
        }

        Debug.LogError($"Block name '{name}' not found!");
        return null;
    }

    public List<int> GetTerrainBlocks()
    {
        List<int> terrainBlocks = new List<int>();
        foreach(BlockEntry block in blocks)
        {
            if(block.isTerrain)
            {
                terrainBlocks.Add(block.blockID);
            }
        }
        return terrainBlocks;
    }
}