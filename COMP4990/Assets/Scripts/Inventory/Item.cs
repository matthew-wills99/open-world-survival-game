using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Tilemaps;
[CreateAssetMenu(menuName = "Scriptable object/Item")]
public class Item : ScriptableObject
{
    public int index;
    public string itemName;
    public bool isTool; // tools are things that gather resources
    public ToolObj toolObj;

    public bool isObj; // obj like sapling crafting table 
    public GameObject obj;

    public bool isWeapon; // weapon
    public GameObject weapon;

    [Header("Only gameplay")]
    public TileBase tile;
    public ItemType type;
    public ActionType actionType;
    public Vector2Int range = new Vector2Int(5,4);

    [Header("Only UI")]
    public bool stackable = true;

    [Header("Both")]
    public Sprite image;

    public enum ItemType{
        BuildingBlock,
        Tool,
        Crafting,
        Weapon
    }

    public enum ActionType {
        Chop,
        Mine,
        None
    }
}
