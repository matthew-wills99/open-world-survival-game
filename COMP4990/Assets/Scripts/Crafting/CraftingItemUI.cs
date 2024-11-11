using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEngine;
using UnityEngine.UI;

public class CraftingItemUI : MonoBehaviour
{
    public Image itemIcon;
    public Button craftButton;
    private CraftingReceipes receipe;
    private InventoryManager inventoryManager;
    private CraftingManager craftingManager;

    public void Setup(CraftingReceipes newReceipe, InventoryManager manager){
        receipe = newReceipe;
        inventoryManager = manager;
        craftingManager = CraftingManager.instance;

        itemIcon.sprite = receipe.resultItem.image;

        craftButton.interactable = craftingManager.hasAllMaterials(receipe);

        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(() => {
            TryCraftItem();
        });
    }

    private void TryCraftItem(){
        if(craftingManager.hasAllMaterials(receipe)){
            bool crafted = craftingManager.CraftItem(receipe.resultItem, inventoryManager);
            if(crafted){
                Debug.Log("Crafted: " + receipe.resultItem.itemName);
            }else{
                Debug.Log("Failed crafting");
            }
        }else{
            Debug.Log("Not enough mats: " + receipe.resultItem.itemName);
        }

        craftButton.interactable = craftingManager.hasAllMaterials(receipe);
        

        
    }

}