using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using UnityEditor;
using UnityEngine;
    //This script is for the inventory. Managing the slots, items, stacks, classes, etc.\
public class InventoryManager : MonoBehaviour
{
    //Allows other scripts to use this script (Building, resources, etc.)
    public static InventoryManager instance;
    public List<Item> startItems;
    public int maxStackedItems = 73;
    public int maxInventorySize = 29;
    public GameObject InventoryItemPrefab;
    public List<InventorySlot> inventorySlots;
   

    [Header("UI")]
    public GameObject inventoryPanel;
    public KeyCode toggleInvnetoryKey = KeyCode.E;
    private bool isInventoryOpen = false;
    int selectedSlot = -1;

    private void Awake(){
        instance = this;
    }
    //Making sure the slot selected at the start is the first slot, then adds starting items.
    private void Start(){
        ChangeSelectedSlot(0);
        foreach (var item in startItems){
            AddItem(item);
        }
        inventoryPanel.SetActive(isInventoryOpen);
    }

    //Changing which slot the user selects
    private void Update(){
        if (Input.GetKeyDown(toggleInvnetoryKey)){
            ToggleInventory();
        }
        //Check for slot selection input (1-8 on keyboard)
        toolbarSelection();
    }

    public void ToggleInventory(){
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);
    }

    private void toolbarSelection(){
        if(Input.inputString != null){
            bool isNumber = int.TryParse(Input.inputString, out int number);
            if (isNumber && number > 0 && number < 8){
                ChangeSelectedSlot(number - 1);
            }
        }
    }
    //Making sure we highlight the correct slot
    void ChangeSelectedSlot(int newValue){
        if(selectedSlot >= 0){
            inventorySlots[selectedSlot].Deselect();
        }
        inventorySlots[newValue].Select();
        selectedSlot = newValue;
    }
    //When "picking up" items this will search for the closets empty slot
    public bool AddItem(Item item){
        //Check if inventory is full
        if(inventorySlots.Count >= maxInventorySize) {
            Debug.LogWarning("Inventory full, cannot add item");
            return false;
        }
        //This for loop is if a stackable item is in the inventory it will add the item to that stack
        for(int i = 0; i < inventorySlots.Count; i++){
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if(itemInSlot != null && 
                itemInSlot.item == item && 
                itemInSlot.count < maxStackedItems &&
                itemInSlot.item.stackable){
                itemInSlot.count++;
                itemInSlot.RefreshCount();
                return true;
            }
        }
        //If there is no stackable item in teh inventory this will find teh closest slot (both hotbar adn backpack)
        for(int i = 0; i < inventorySlots.Count; i++){
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if(itemInSlot == null){
                SpawnNewItem(item, slot);
                return true;
            }
        }
        return false;
    }

    //once the slot found from above this will add the item to the slot
    void SpawnNewItem(Item item, InventorySlot slot){
        GameObject newItemGo = Instantiate(InventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGo.GetComponent<InventoryItem>();
        inventoryItem.InitialiseItem(item);
    }

    //This checks if the item that is currently being used by the player is being used.
    public Item GetSelectedItem(){
        if (selectedSlot < 0 || selectedSlot >= inventorySlots.Count) return null;

        InventorySlot slot = inventorySlots[selectedSlot];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
        if(itemInSlot != null){
            return itemInSlot.item;
        }

        return null;
    }

    public bool HasItem (Item item, int count){
        int itemCount = 0;
        for(int i = 0; i < inventorySlots.Count; i++){
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if(itemInSlot != null && itemInSlot.item == item){
                itemCount += itemInSlot.count;
                if(itemCount > count){
                    return true;
                }
            }
        }
        return false;
    }

    public bool RemoveItem(Item item)
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null && 
            itemInSlot.item == item && 
            item.type != Item.ItemType.Tool)
            {
                if (itemInSlot.count > 1)
                {
                    itemInSlot.count--;
                    itemInSlot.RefreshCount();
                    return true;
                }
                else
                {
                    Destroy(itemInSlot.gameObject);
                    return true;
                }
            
            }
        } 
        return false;  
    } 

    public bool RemoveItemForCrafting(Item item, int count)
    {
        Debug.Log(count);
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null && 
            itemInSlot.item == item && 
            item.type != Item.ItemType.Tool)
            {
                if (itemInSlot.count >= count)
                {
                    itemInSlot.count -= count;
                    if(itemInSlot.count == 0){
                        Destroy(itemInSlot.gameObject);
                    }else{
                        itemInSlot.RefreshCount();
                    }
                    return true;
                }
                else
                {
                    count -= itemInSlot.count;
                    Destroy(itemInSlot.gameObject);
                    return true;
                }
            
            }
        } 
        return false;  
    }     

}

    


