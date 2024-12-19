using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static EUtils;

/// <summary>
/// A tool is an item that is capable of destroying terrain, objects, player-placed tiles
/// and things
/// A tool will be applied to a resource tile (rock, tree, cactus)
/// A tool will have:
///     a damage multiplier
///     a resource multiplier
///     a tool type
///     a sprite index found in TileIndex
/// </summary>
[CreateAssetMenu(menuName = "Scriptable object/ToolObj")]
public class ToolObj : ScriptableObject
{
    public float damageMultiplier;
    public float resourceMultiplier;
    public ETool toolType;
    public int spriteIndex;

    public float Hit()
    {
        return 1 * damageMultiplier;
    }
}
