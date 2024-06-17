using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable object/CraftingRecipe")]
public class CraftingReceipes : ScriptableObject
{
    public Item resultItem;
    public List<Ingredient> ingredients;

    [Serializable]
    public class Ingredient{
        public Item item;
        public int count;
    }
}
