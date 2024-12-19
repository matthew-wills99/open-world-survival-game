using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static EUtils;

public class UpdateableBlocks : MonoBehaviour
{
    Dictionary<Vector3Int, Placeable> blocks;
    Dictionary<Vector3Int, PlaceableBlock> placeableBlocks;

    void Start()
    {
        blocks = new Dictionary<Vector3Int, Placeable>();
        placeableBlocks = new Dictionary<Vector3Int, PlaceableBlock>();
    }

    public List<PlaceableBlock> GetPlaceableBlocks()
    {
        return placeableBlocks.Values.ToList();
    }

    public void AddBlock(Vector3Int pos, Placeable block)
    {
        if(!blocks.ContainsKey(pos))
        {
            blocks.Add(pos, block);
            placeableBlocks.Add(pos, new PlaceableBlock(block.PID, pos.x, pos.y, pos.z));
        }
        
    }

    public void RemoveBlock(Vector3Int pos)
    {
        if(blocks.ContainsKey(pos))
        {
            blocks.Remove(pos);
            placeableBlocks.Remove(pos);
        }
    }

    public Placeable GetBlock(Vector3Int pos)
    {
        if(blocks.ContainsKey(pos))
        {
            return blocks[pos];
        }
        return null;
    }

}
