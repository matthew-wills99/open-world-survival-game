using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftingItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image itemIcon;
    public Button craftButton;
    private CraftingReceipes recipe;
    private InventoryManager inventoryManager;
    private CraftingManager craftingManager;

    bool isPointerOverItem;

    public void Setup(CraftingReceipes newRecipe, InventoryManager manager)
    {
        recipe = newRecipe;
        inventoryManager = manager;
        craftingManager = CraftingManager.instance;

        // Set the item icon
        itemIcon.sprite = recipe.resultItem.image;

        // Check if the player has the required materials
        bool hasMaterials = craftingManager.hasAllMaterials(recipe);

        // Update the UI state based on material availability
        UpdateUIState(hasMaterials);

        // Set up the button click action
        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(() => TryCraftItem());
    }

    private void TryCraftItem()
    {
        if (craftingManager.hasAllMaterials(recipe))
        {
            bool crafted = craftingManager.CraftItem(recipe.resultItem, inventoryManager);
            if (crafted)
            {
                Debug.Log("Crafted: " + recipe.resultItem.itemName);
            }
            else
            {
                Debug.Log("Failed crafting.");
            }
        }
        else
        {
            Debug.Log("Not enough materials: " + recipe.resultItem.itemName);
        }

        // Refresh the UI state after attempting to craft
        bool hasMaterials = craftingManager.hasAllMaterials(recipe);
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(TooltipManager.Instance != null)
        {
            TooltipManager.Instance.ShowTooltip("yes");
            isPointerOverItem = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
            isPointerOverItem = false;
        }
    }

    void Update()
    {
        if(isPointerOverItem && TooltipManager.Instance != null)
        {
            TooltipManager.Instance.UpdatePosition();
        }
    }
}