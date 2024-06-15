using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct itemAmount{
    public Item Item;
    [Range(1,4)]
    public int Amount;
}
[CreateAssetMenu]
public class CraftingReceipes : ScriptableObject
{
    public List<itemAmount> Materials;
    public List<itemAmount> Results;
}
