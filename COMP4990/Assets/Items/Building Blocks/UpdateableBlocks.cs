using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpdateableBlocks : MonoBehaviour
{
    Dictionary<Vector3Int, Placeable> blocks;

    void Start()
    {
        blocks = new Dictionary<Vector3Int, Placeable>();
    }

    public void AddBlock(Vector3Int pos, Placeable block)
    {
        if(!blocks.ContainsKey(pos))
        {
            blocks.Add(pos, block);
        }
        
    }

    public void RemoveBlock(Vector3Int pos)
    {
        if(blocks.ContainsKey(pos))
        {
            blocks.Remove(pos);
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
