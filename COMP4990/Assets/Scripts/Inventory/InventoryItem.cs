using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
//This script is for items and stacking items, and dragging items
public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
   

    [Header("UI")]
    public Image image;
    public Text countText;
    [HideInInspector] public Transform parentAfterDrag; //Dragging item movement
    [HideInInspector] public int count = 1; 
    [HideInInspector] public Item item; //Whats the item homes?
   
    //Item Sprites are loaded in to the correct type
    //The stack count is refreshed
    public void InitialiseItem(Item newItem){
        item = newItem;
        image.sprite = newItem.image;
        RefreshCount();
    }

    //Displaying the correct stack count
    public void RefreshCount(){
        countText.text = count.ToString();
        bool textActive = count > 1;
        countText.gameObject.SetActive(textActive);
    }
    //Starting the dragging, Raycast off means we can move it into different slots. SetAsLastSibling means its going to stay above all slots
    public void OnBeginDrag(PointerEventData eventData){
        Debug.Log("Begin Drag");
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
        
    }

    //Dragging the dragging lol
    public void OnDrag(PointerEventData eventData){
        Debug.Log("Dragging");
        transform.position = Input.mousePosition;
        
    }

    //Raycast true so we can place it on the right item slot
    public void OnEndDrag(PointerEventData eventData){
        Debug.Log("End Drag");
        transform.SetParent(parentAfterDrag);
        image.raycastTarget = true;
    }
    
}
