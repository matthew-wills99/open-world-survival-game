using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
/*
This Script is for Inventory Items. It manages inventory items, including stacking, dragging, and showing tooltips.
*/
public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    public Image image;
    public TMP_Text countText;
    [HideInInspector] public Transform parentAfterDrag; //Dragging item movement
    [HideInInspector] public int count = 1; 
    public Item item; //Refrence to the item with its inventory slot

    private bool isPointerOverItem;

    //Initialize item with its properties after updating the crafting ui
    public void InitialiseItem(Item newItem){
        item = newItem;
        image.sprite = newItem.image;
        CraftingBarUI.instance.UpdateCraftingBar();
        RefreshCount();
    }

    //Displaying the correct stack count
    public void RefreshCount(){
        countText.text = count.ToString();
        countText.raycastTarget = false;
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

    //Dragging an item
    public void OnDrag(PointerEventData eventData){
        Debug.Log("Dragging");
        transform.position = Input.mousePosition;
        
    }
    //This moves the item to the correct Item slot once OnDrag is over
    //Raycast true so we can place it on the right item slot
    public void OnEndDrag(PointerEventData eventData){
        Debug.Log($"End Drag : {transform.name}");
        transform.SetParent(parentAfterDrag);
        image.raycastTarget = true;
    }
    
    //Pointer entring the item slot
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(TooltipManager.Instance != null)
        {
            TooltipManager.Instance.ShowTooltip(item.itemName);
            isPointerOverItem = true;
        }
    }
    //Pointer exiting itemslot
    public void OnPointerExit(PointerEventData eventData)
    {
        if(TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
            isPointerOverItem = false;
        }
    }

    //Update tooltip position if the pointer is over the item
    void Update()
    {
        if(isPointerOverItem && TooltipManager.Instance != null)
        {
            TooltipManager.Instance.UpdatePosition();
        }
    }
}