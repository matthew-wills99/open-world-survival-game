using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUI : MonoBehaviour
{
    public Transform content;
    public GameObject craftingItemPrefab;
    private InventoryManager inventoryManager;

    void Start()
    {
        inventoryManager = InventoryManager.instance;
        LoadAllCraftingRecipes();
    }

    void LoadAllCraftingRecipes()
    {
        List<CraftingReceipes> allRecipes = CraftingManager.instance.receipes;
        foreach (var recipe in allRecipes)
        {
            GameObject itemUI = Instantiate(craftingItemPrefab, content);
            CraftingItemUI craftingItemUI = itemUI.GetComponent<CraftingItemUI>();
            craftingItemUI.Setup(recipe, inventoryManager);
        }
    }
}