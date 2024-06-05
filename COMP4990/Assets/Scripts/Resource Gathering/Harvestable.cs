using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class Harvestable : MonoBehaviour
{
   public Item.ActionType RequiredAction;
   public Item itemToAdd;
   public float cooldownTime = 5.0f;
   private bool onCooldown = false;
   private Renderer objectRenderer;
   private Color objectColour;
   private Color cooldownColour = Color.black;

    void start(){
        objectRenderer = GetComponent<SpriteRenderer>();
        if(objectRenderer != null){
            objectColour = objectRenderer.material.color;
        }else{
            Debug.LogError("ObjectRendere Is Null");
        }
    }

    void OnMouseDown(){


        if(onCooldown){
            Debug.Log("On Cooldown");
            return;
        }

        Item selectedItem = InventoryManager.instance?.GetSelectedItem();
        if (selectedItem != null && selectedItem.actionType == RequiredAction)
        {
            Debug.Log("Item" + gameObject);
            StartCoroutine(Cooldown());
            InventoryManager.instance.AddItem(itemToAdd);
        }else{
            Debug.Log("You need a " + RequiredAction.ToString());
        }

    }

    private IEnumerator Cooldown(){
        
        onCooldown = true;
        if(objectRenderer != null){
            objectRenderer.material.color = cooldownColour;
        }

        yield return new WaitForSeconds(cooldownTime);
        onCooldown = false;
        if(objectRenderer != null){
            objectRenderer.material.color = objectColour;
        }

    }

}
