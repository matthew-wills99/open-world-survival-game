using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//This script is only for testing the Inventory
public class DemoScript : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public Item[] itemsToPickup;

    //PickupItem detects if an item has been added to the inventory
    public void PickupItem(int id){
        bool result = inventoryManager.AddItem(itemsToPickup[id]);
        if (result == true){
            Debug.Log("Item Added");
        }else{
            Debug.Log("Item not Added");
        }
    }

    //Get Selected Item detects what item has been added to the inventory
    public void GetSelectedItem(){
        Item recievedItem = inventoryManager.GetSelectedItem();
        if (recievedItem != null) {
            Debug.Log("Received" + recievedItem);
        }else{
            Debug.Log("No item received!");
        }
    }

    //UseSelectedItem detects if an item has been used or not.
    public void UseSelectedItem(){
        Item recievedItem = inventoryManager.GetSelectedItem();
        if (recievedItem != null) {
            Debug.Log("Used " + recievedItem);
        }else{
            Debug.Log("No Item Used!");
        }
    }

}
