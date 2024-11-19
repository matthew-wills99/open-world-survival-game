using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
//This script is for items and stacking items, and dragging items
public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    public Image image;
    public TMP_Text countText;
    [HideInInspector] public Transform parentAfterDrag; //Dragging item movement
    [HideInInspector] public int count = 1; 
    [HideInInspector] public Item item; //Whats the item homes?

    private bool isPointerOverItem;

    //Item Sprites are loaded in to the correct type
    //The stack count is refreshed
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

    //Dragging the dragging lol
    public void OnDrag(PointerEventData eventData){
        Debug.Log("Dragging");
        transform.position = Input.mousePosition;
        
    }

    //Raycast true so we can place it on the right item slot
    public void OnEndDrag(PointerEventData eventData){
        Debug.Log($"End Drag : {transform.name}");
        transform.SetParent(parentAfterDrag);
        image.raycastTarget = true;
    }
    
    // hi yusuf
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(TooltipManager.Instance != null)
        {
            TooltipManager.Instance.ShowTooltip(item.itemName);
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