using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using static Utils;

/// <summary>
/// A resource object is an object that you can collect resources from (rock, tree, cactus)
/// a resource object is made up of:
///     a health value
///     a resources per hit amount
///     a designated type of tool (pickaxe, axe)
///     the name of the resource object
///     the index of the resource that is dropped (TileIndex)
/// </summary>
public class ResourceObj : MonoBehaviour
{
    public float health;
    public float resourcesPerHit;
    public ETool toolType;
    public List<ItemDrop> drops;
    public int resourceIndex;
    
    public ResourceObj(float health, float resourcesPerHit, ETool toolType, List<ItemDrop> drops, int resourceIndex)
    {
        this.health = health;
        this.resourcesPerHit = resourcesPerHit;
        this.toolType = toolType;
        this.drops = drops;
        this.resourceIndex = resourceIndex;
    }

    public List<ItemDrop> Hit(ToolObj toolObj)
    {
        List<ItemDrop> collectedDrops = new List<ItemDrop>();
        if(toolObj.toolType == toolType)
        {
            health -= toolObj.Hit();
            foreach(var drop in drops)
            {
                if(Random.Range(0, 100) < drop.dropChance)
                {
                    int yield = Mathf.RoundToInt(1 * toolObj.resourceMultiplier);
                    collectedDrops.Add(new ItemDrop(drop.item, yield, drop.dropChance));
                    //Debug.Log($"Hit: {drop.item}, got: {yield}, remaining health: {health}");
                }
            }
        }
        else
        {
            return null;
        }
        return collectedDrops;
    }
}