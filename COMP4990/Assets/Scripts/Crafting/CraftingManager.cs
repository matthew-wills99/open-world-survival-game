using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    public List<CraftingReceipes> recipes;
    public bool CraftItem(Item resultItem, InventoryManager inventory){
        CraftingReceipes recipe = recipes.Find(r => r.resultItem == resultItem);
        if (recipe == null){
            Debug.Log("Recipe not found.");
            return false;
        } 

        foreach (var ingredient in recipe.ingredients){
           
            if(!inventory.HasItem(ingredient.item, ingredient.count-1)){
                return false;
            }
        }
        
        foreach (var ingredient in recipe.ingredients){
            inventory.RemoveItemForCrafting(ingredient.item, ingredient.count);
        }

        inventory.AddItem(resultItem);
        return true;
    }
}
