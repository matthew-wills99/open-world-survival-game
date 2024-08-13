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
    public EResource resource;
    public int resourceIndex;
    
    public ResourceObj(float health, float resourcesPerHit, ETool toolType, EResource resource, int resourceIndex)
    {
        this.health = health;
        this.resourcesPerHit = resourcesPerHit;
        this.toolType = toolType;
        this.resource = resource;
        this.resourceIndex = resourceIndex;
    }

    public int Hit(ToolObj toolObj)
    {
        if(toolObj.toolType == toolType)
        {
            health -= toolObj.Hit();
            int yield = Mathf.RoundToInt(1 * toolObj.resourceMultiplier);
            Debug.Log($"Hit: {resource}, got: {yield}, remaining health: {health}");
            return yield;
        }
        return 0;
    }
}
