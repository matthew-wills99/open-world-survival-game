using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public int maxStackedItems = 4;
    public GameObject InventoryItemPrefab;
    public InventorySlot[] inventorySlots;

    int selectedSlot = -1;

    private void Start(){
        ChangeSelectedSlot(0);
    }

    private void Update(){
        if(Input.inputString != null){
            bool isNumber = int.TryParse(Input.inputString, out int number);
            if (isNumber && number > 0 && number < 8){
                ChangeSelectedSlot(number - 1);
            }
        }
    }
    void ChangeSelectedSlot(int newValue){
        if(selectedSlot >= 0){
            inventorySlots[selectedSlot].Deselect();
        }
        inventorySlots[newValue].Select();
        selectedSlot = newValue;
    }
    //When "picking up" items this will search for the closets empty slot
    public bool AddItem(Item item){

        for(int i = 0; i < inventorySlots.Length; i++){
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if(itemInSlot != null && 
            itemInSlot.item == item && 
            itemInSlot.count < maxStackedItems &&
            itemInSlot.item.stackable == true){
                itemInSlot.count++;
                itemInSlot.RefreshCount();
                return true;
            }
        }

        for(int i = 0; i < inventorySlots.Length; i++){
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
}
