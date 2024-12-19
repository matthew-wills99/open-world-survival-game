
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static EUtils;
using static MiningSystem;

public class Tools : MonoBehaviour 
{
    public class PickaxeObj : ToolObj
    {
        public PickaxeObj()
        {
            damageMultiplier = 1.0f; 
            resourceMultiplier = 1.0f;
            toolType = ETool.Pickaxe; 
            spriteIndex = 1;
        }
    }

    public class AxeObj : ToolObj
    {
        public AxeObj()
        {
            damageMultiplier = 1.0f; 
            resourceMultiplier = 1.0f;
            toolType = ETool.Axe; 
            spriteIndex = 1;
        }
    }
}

