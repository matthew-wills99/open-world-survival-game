using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Utils;
/*
    This script is the inventory manager, used inside the unity as an object. This initializes and displays the Inventory with the correct information using other scripts
    aswell.
*/
public class InventoryManager : MonoBehaviour
{
    public InventoryItemIndex inventoryItemIndex;

    //Allows other scripts to use this script (Building, resources, etc.)
    public static InventoryManager instance;
    public List<Item> startItems;
    public int maxStackedItems = 73;
    public int maxInventorySize = 29;
    public GameObject InventoryItemPrefab;
    private List<InventorySlot> inventorySlots;

    public Transform inventoryContainer;
    public Transform toolbarContainer;

    public bool isLoaded = false;
   

    [Header("UI")]
    public GameObject inventoryPanel;
    public KeyCode toggleInvnetoryKey = KeyCode.E;
    private bool isInventoryOpen = false;
    int selectedSlot = -1;

    private void GetInventorySlots()
    {
        inventorySlots = new List<InventorySlot>();

        // Populate toolbar slots
        foreach (Transform child in toolbarContainer)
        {
            InventorySlot slot = child.GetComponent<InventorySlot>();
            if (slot != null)
            {
                inventorySlots.Add(slot);
            }
        }

        // Populate main inventory slots
        foreach (Transform child in inventoryContainer)
        {
            InventorySlot slot = child.GetComponent<InventorySlot>();
            if (slot != null)
            {
                inventorySlots.Add(slot);
            }
        }
    }
    private void Awake(){
        instance = this;
        GetInventorySlots();

    }
    //Making sure the slot selected at the start is the first slot, then adds starting items.
    private void Start(){
        GetInventorySlots();
        ChangeSelectedSlot(0);
        inventoryPanel.SetActive(isInventoryOpen);

        foreach (var item in startItems){
            Debug.Log("Now");
            AddItem(item);
        }

        CraftingBarUI.instance.UpdateCraftingBar();

        if(!isLoaded)
        {
            isLoaded = true;
        }
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
    public void AddItem(Item item, int count){
        for(int c = 0; c < count; c++)
        {
            if(!AddItem(item))
            {
                return;
            }
        }
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
                CraftingBarUI.instance.UpdateCraftingBar();
                return true;
            }
        }
        //If there is no stackable item in teh inventory this will find teh closest slot (both hotbar adn backpack)
        for(int i = 0; i < inventorySlots.Count; i++){
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if(itemInSlot == null){
                SpawnNewItem(item, slot);
                CraftingBarUI.instance.UpdateCraftingBar();
                return true;
            }
        }
        CraftingBarUI.instance.UpdateCraftingBar();
        return false;
    }

    //once the slot found from above this will add the item to the slot
    void SpawnNewItem(Item item, InventorySlot slot){
        CraftingBarUI.instance.UpdateCraftingBar();
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
       // Debug.Log("Checking for item: " + item.itemName);
        //Debug.Log("Inventory Slots Count: " + inventorySlots.Count);
        int itemCount = 0;
        for(int i = 0; i < inventorySlots.Count; i++){
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if(itemInSlot != null && itemInSlot.item == item){
                Debug.Log("Found item in slot");
                itemCount += itemInSlot.count;
                if(itemCount > count){
                    return true;
                }
            }
        }
        //Debug.LogWarning("Item not found: " + item.itemName);
        return false;
    }

    //Removes item from inventory for crafting and such
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
                    CraftingBarUI.instance.UpdateCraftingBar();
                    return true;
                }
                else
                {
                    Destroy(itemInSlot.gameObject);
                    CraftingBarUI.instance.UpdateCraftingBar();
                    return true;
                }
            
            }
        } 
        return false;  
    } 
    //Updates inventory and crafting bar when crafting
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
                    CraftingBarUI.instance.UpdateCraftingBar();

                    return true;
                }
                else
                {
                    count -= itemInSlot.count;
                    Destroy(itemInSlot.gameObject);
                    CraftingBarUI.instance.UpdateCraftingBar();

                    return true;
                }
            
            }
        } 
        return false;  
    }     
    //Saves the inventory for differnt worlds
    public List<CInventoryItem> GetSavedInventory()
    {
        List<CInventoryItem> savedItems = new List<CInventoryItem>();

        GetInventorySlots();
        foreach(InventorySlot slot in inventorySlots)
        {
            if(slot == null)
            {
                Debug.Log("slot null");
            }
            if(slot.GetComponentInChildren<InventoryItem>() == null)
            {
                Debug.Log("inv item null");
            }
            if(slot != null && slot.GetComponentInChildren<InventoryItem>() != null)
            {
                InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
                Item item = itemInSlot.item;
                int count = itemInSlot.count;
                Debug.Log($"Saved: {item.name}, {count}");
                savedItems.Add(new CInventoryItem(item.index, count));
            }
        }
        return savedItems;
    }
    //Loads the inventory
    public void LoadInventory(List<CInventoryItem> items)
    {
        GetInventorySlots();
        foreach(CInventoryItem item in items)
        {
            AddItem(inventoryItemIndex.GetItemById(item.Idx), item.Count);
        }
    }
}