using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager instance;
    public List<CraftingReceipes> receipes;

    private void Awake(){
        instance = this;
        LoadAllCraftingReceipes();
    }

    private void LoadAllCraftingReceipes()
    {
        // Assuming you have a folder named "Resources/Crafting Recipe" in the Assets folder
        CraftingReceipes[] loadedReceipes = Resources.LoadAll<CraftingReceipes>("Crafting Recipe");
        if(loadedReceipes.Length > 0){
            receipes.AddRange(loadedReceipes);
            Debug.Log($"Loaded {loadedReceipes.Length} crafting receipes");
        }else{
            Debug.LogWarning("No crafting receipese found in rsources");
        }
    }
    public bool CraftItem(Item resultItem, InventoryManager inventory){
        CraftingReceipes receipe = receipes.Find(r => r.resultItem == resultItem);
        if (receipe == null){
            Debug.Log("Recipe not found.");
            return false;
        } 

        if(hasAllMaterials(receipe)){
            foreach (var ingredient in receipe.ingredients){
                inventory.RemoveItemForCrafting(ingredient.item, ingredient.count);
            }

            inventory.AddItem(resultItem);
                return true;
        }
        return false;
    }

    public bool hasAllMaterials(CraftingReceipes recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            if (!InventoryManager.instance.HasItem(ingredient.item, ingredient.count))
            {
                return false;
            }
        }
        return true;
    }

    public List<CraftingReceipes> GetAvailableRecipes()
    {
        List<CraftingReceipes> availableReceipes = new List<CraftingReceipes>();

        foreach (var recipe in receipes)
        {
            if (hasAllMaterials(recipe))
            {
                availableReceipes.Add(recipe);
            }
        }

        return availableReceipes;
    }
}