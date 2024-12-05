using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingBarUI : MonoBehaviour
{
    public Transform content; // Assign the 'Content' GameObject from the Scroll View
    public GameObject craftableItemPrefab;
    public InventoryManager inventoryManager;
    private CraftingManager craftingManager;
    public static CraftingBarUI instance;
    void Awake()
    {
        // Singleton pattern implementation
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Start()
    {
        craftingManager = CraftingManager.instance;
        UpdateCraftingBar();
    }

    public void UpdateCraftingBar()
    {
        //Debug.Log("Updating Crafting Bar...");

        // Clear existing items in the crafting bar
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // Get the full list of crafting recipes
        List<CraftingReceipes> allRecipes = craftingManager.receipes;
        List<CraftingReceipes> craftableRecipes = new List<CraftingReceipes>();

        // Check which recipes the player can craft
        foreach (var recipe in allRecipes)
        {
            if (craftingManager.hasAllMaterials(recipe))
            {
                craftableRecipes.Add(recipe);
            }
        }

        // If there are no craftable recipes, hide the crafting bar
        if (craftableRecipes.Count == 0)
        {
            //Debug.Log("No craftable recipes. Hiding crafting bar.");
            gameObject.SetActive(false);
            return;
        }

        // Show the crafting bar and display only craftable recipes
        gameObject.SetActive(true);
        //Debug.Log($"Displaying {craftableRecipes.Count} craftable recipes.");

        foreach (var recipe in craftableRecipes)
        {
            // Instantiate a UI element for each craftable recipe
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
