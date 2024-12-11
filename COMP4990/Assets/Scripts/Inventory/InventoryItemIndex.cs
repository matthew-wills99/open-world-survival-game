using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
    This is the Item Index class.  It manages and retrieves items in the inventory system we have implemented. 
    It maps unique Item IDs to their corresponding item instances.
*/
public class InventoryItemIndex : MonoBehaviour
{
    public Item wood;
    public Item stick;
    public Item treeBark;
    public Item treeSapling;
    public Item fiber;
    public Item stone;
    public Item flint;
    public Item rawCactus;
    public Item sandstone;
    public Item obsidian;
    public Item clay;
    public Item ironOre;
    public Item copperOre;
    public Item goldNugget;
    public Item ironIngot;
    public Item copperIngot;
    public Item goldIngot;
    public Item workbench;
    public Item torch;
    public Item campfire;
    public Item flintPickaxe;
    public Item flintHatchet;
    public Item stonePickaxe;
    public Item stoneHatchet;
    public Item forge;
    public Item ironPickaxe;
    public Item ironAxe;
    public Item chest;
    public Item treeSap;
    public Item cactusSapling;
    public Item woodenFence;
    public Item woodenGate;
    public Item ironLongsword;

    Dictionary<int, Item> idx;

    void Awake()
    {
        idx = new Dictionary<int, Item>()
        {
            {1, wood},
            {2, stick},
            {3, treeBark},
            {4, treeSapling},
            {5, fiber},
            {6, stone},
            {7, flint},
            {8, rawCactus},
            {9, sandstone},
            {11, obsidian},
            {12, clay},
            {13, ironOre},
            {14, copperOre},
            {15, goldNugget},
            {16, ironIngot},
            {17, copperIngot},
            {18, goldIngot},
            {19, workbench},
            {20, torch},
            {21, campfire},
            {22, flintPickaxe},
            {23, flintHatchet},
            {24, stonePickaxe},
            {25, stoneHatchet},
            {27, forge},
            {28, ironPickaxe},
            {29, ironAxe},
            {32, chest},
            {33, treeSap},
            {34, cactusSapling},
            {101, woodenFence},
            {102, woodenGate},
            {141, ironLongsword},
        };
    }

    public Item GetItemById(int id)
    {
        return idx[id];
    }
}
