using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingBarUI : MonoBehaviour
{
    public Transform content; // Assign the 'Content' GameObject from the Scroll View
    public GameObject craftableItemPrefab;
    private InventoryManager inventoryManager;
    private CraftingManager craftingManager;

    void Start()
    {
        inventoryManager = InventoryManager.instance;
        craftingManager = CraftingManager.instance;
        UpdateCraftingBar();
    }

 public void UpdateCraftingBar()
{
    Debug.Log("Updating Crafting Bar...");

    // Clear existing items in the crafting bar
    foreach (Transform child in content)
    {
        Destroy(child.gameObject);
    }

    // Get the full list of crafting recipes (do not filter by materials)
    List<CraftingReceipes> allRecipes = craftingManager.receipes;
    Debug.Log($"Displaying {allRecipes.Count} crafting recipes in the bar.");

    foreach (var recipe in allRecipes)
    {
        // Instantiate a UI element for each recipe
        GameObject itemUI = Instantiate(craftableItemPrefab, content);
        Image itemIcon = itemUI.GetComponent<Image>();
        Button craftButton = itemUI.GetComponent<Button>();

        // Set the icon of the craftable item
        if (itemIcon != null)
        {
            itemIcon.sprite = recipe.resultItem.image;
        }
        else
        {
            Debug.LogWarning("ItemIcon component is missing on the CraftableItemUI prefab.");
        }

        // Set up the button click action to attempt crafting
        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(() => CraftItem(recipe));

        Debug.Log($"Added craftable item to bar: {recipe.resultItem.itemName}");
    }
}

    private void CraftItem(CraftingReceipes recipe)
    {
        if (craftingManager.hasAllMaterials(recipe))
        {
            bool crafted = craftingManager.CraftItem(recipe.resultItem, inventoryManager);
            if (crafted)
            {
                Debug.Log($"Crafted: {recipe.resultItem.itemName}");
                UpdateCraftingBar(); // Refresh the crafting bar after successful crafting
            }
            else
            {
                Debug.Log("Failed to craft item.");
            }
        }
        else
        {
            Debug.Log("Not enough materials to craft: " + recipe.resultItem.itemName);
        }
    }
}