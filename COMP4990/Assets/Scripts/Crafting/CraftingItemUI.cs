using UnityEngine;
using UnityEngine.UI;

public class CraftingItemUI : MonoBehaviour
{
    public Image itemIcon;
    public Button craftButton;
    private CraftingReceipes receipe;
    private InventoryManager inventoryManager;
    private CraftingManager craftingManager;

    public void Setup(CraftingReceipes newReceipe, InventoryManager manager)
    {
        receipe = newReceipe;
        inventoryManager = manager;
        craftingManager = CraftingManager.instance;

        // Set the item icon
        itemIcon.sprite = receipe.resultItem.image;

        // Check if the player has the required materials
        bool hasMaterials = craftingManager.hasAllMaterials(receipe);

        // Update the UI state based on material availability
        UpdateUIState(hasMaterials);

        // Set up the button click action
        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(() => TryCraftItem());
    }

    private void TryCraftItem()
    {
        if (craftingManager.hasAllMaterials(receipe))
        {
            bool crafted = craftingManager.CraftItem(receipe.resultItem, inventoryManager);
            if (crafted)
            {
                Debug.Log("Crafted: " + receipe.resultItem.itemName);
            }
            else
            {
                Debug.Log("Failed crafting.");
            }
        }
        else
        {
            Debug.Log("Not enough materials: " + receipe.resultItem.itemName);
        }

        // Refresh the UI state after attempting to craft
        bool hasMaterials = craftingManager.hasAllMaterials(receipe);
        UpdateUIState(hasMaterials);
    }

    private void UpdateUIState(bool hasMaterials)
    {
        // Faded color when the player doesn't have enough resources
        Color fadedColor = new Color(1f, 1f, 1f, 0.3f); // 30% opacity (faded)
        Color fullColor = Color.white; // Full opacity

        // Set the icon color based on whether the player has the required materials
        itemIcon.color = hasMaterials ? fullColor : fadedColor;

        // Disable the button if the player lacks materials
        craftButton.interactable = hasMaterials;
    }
}