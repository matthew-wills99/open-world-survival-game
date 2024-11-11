
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using static Tools;



public class MiningSystem : MonoBehaviour
{
    private Transform highlight;
    private Transform currentlySelectedObject;
    public MapManager mapManager;
    public InventoryManager inventoryManager;

    private bool miningMode = false;

    Item selectedItem;

    private ToolObj pickaxe;
    private ToolObj axe;

    //private ToolObj equippedTool;

    void Awake()
    {
        pickaxe = ScriptableObject.CreateInstance<PickaxeObj>();
        axe = ScriptableObject.CreateInstance<AxeObj>();
    }

    void Update()
    {
        // mining
        if(Input.GetKeyDown(KeyCode.M))
        {
            miningMode = !miningMode;

            if(!miningMode) // if exiting build mode
            {
                if(highlight != null)
                {
                    highlight.GetComponent<Hover>().EndHover();
                    highlight = null;
                }
            }
            //Debug.Log($"Mining mode set to: {miningMode}");
        }

        if(miningMode)
        {
            MiningMode();
        }
    }

    void MiningMode()
    {
        if(highlight != null)
        {
            highlight.GetComponent<Hover>().EndHover();
            highlight = null;
        }
        else
        {
            // hasnt been hovering for more than 1 frame
            // want to keep track of which gameobject is being hovered, on mb1 delete it
            if(currentlySelectedObject != null)
            {
                currentlySelectedObject = null;
            }
        }

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(GetMousePosition());
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);
        // closest
        RaycastHit2D hit = new RaycastHit2D();
        float highestOrder = -Mathf.Infinity;

        foreach(RaycastHit2D h in hits)
        {
            if(h.collider != null && h.collider.CompareTag("Selectable"))
            {

                if(h.transform.name == "Upper" || h.transform.name == "Lower")
                {
                    float order = h.collider.transform.parent.GetComponent<SortingGroup>().sortingOrder;
                    if(order > highestOrder)
                    {
                        highestOrder = order;
                        hit = h;
                    }
                }
                else
                {
                    float order = h.collider.GetComponent<SortingGroup>().sortingOrder;
                    if(order > highestOrder)
                    {
                        highestOrder = order;
                        hit = h;
                    }
                }
            }
        }

        if (hit.collider != null)
        {
            // has selectable tag
            if(hit.collider.transform.CompareTag("Selectable"))
            {
                // if its a tree
                if(hit.transform.name == "Upper" || hit.transform.name == "Lower")
                {
                    // if it has a parent
                    if(hit.transform.parent != null)
                    {
                        // start hover on tree
                        highlight = hit.transform.parent.transform;
                        hit.transform.parent.gameObject.GetComponent<Hover>().StartHover();
                        currentlySelectedObject = hit.transform.parent;
                    }
                }
                // not a tree
                else
                {
                    highlight = hit.transform;
                    hit.transform.gameObject.GetComponent<Hover>().StartHover();
                    currentlySelectedObject = hit.transform;
                }
            }
        }

        if(Input.GetMouseButtonDown(0))
        {
            selectedItem = inventoryManager.GetSelectedItem();
            if(selectedItem == null)
            {
                Debug.Log("No item");
                return;
            }
            else if(selectedItem.toolObj == null)
            {
                Debug.Log("No tool");
                return;
            }
            else
            {
                Debug.Log($"{selectedItem.toolObj.name}");
            }
            if(currentlySelectedObject != null)
            {
                //Debug.Log($"Clicking {currentlySelectedObject.name}");
                if(currentlySelectedObject.GetComponent<RockObj>())
                {
                    if(currentlySelectedObject.GetComponent<ResourceObj>())
                    {
                        List<ItemDrop> drops = currentlySelectedObject.GetComponent<ResourceObj>().Hit(selectedItem.toolObj);
                        if(drops == null)
                        {
                            return;
                        }
                        foreach(ItemDrop drop in drops)
                        {
                            inventoryManager.AddItem(drop.item, drop.quantity);
                        }
                        if(currentlySelectedObject.GetComponent<ResourceObj>().health <= 0)
                        {
                            (int cx, int cy, int tx, int ty) c = currentlySelectedObject.GetComponent<RockObj>().GetCoordinates();
                            mapManager.DestroyObj(c.cx, c.cy, c.tx, c.ty);
                            currentlySelectedObject.GetComponent<RockObj>().Destroy();
                        }
                    }
                }
                else if(currentlySelectedObject.GetComponent<TreeObj>())
                {
                    if(currentlySelectedObject.GetComponent<ResourceObj>())
                    {
                        List<ItemDrop> drops = currentlySelectedObject.GetComponent<ResourceObj>().Hit(selectedItem.toolObj);
                        if(drops == null)
                        {
                            return;
                        }
                        foreach(ItemDrop drop in drops)
                        {
                            inventoryManager.AddItem(drop.item, drop.quantity);
                        }
                        if(currentlySelectedObject.GetComponent<ResourceObj>().health <= 0)
                        {
                            (int cx, int cy, int tx, int ty) c = currentlySelectedObject.GetComponent<TreeObj>().GetCoordinates();
                            mapManager.DestroyObj(c.cx, c.cy, c.tx, c.ty);
                            currentlySelectedObject.GetComponent<TreeObj>().Destroy();
                        }
                    }
                }
                else if(currentlySelectedObject.GetComponent<TCactusObj>())
                {
                    if(currentlySelectedObject.GetComponent<ResourceObj>())
                    {
                        List<ItemDrop> drops = currentlySelectedObject.GetComponent<ResourceObj>().Hit(selectedItem.toolObj);
                        if(drops == null)
                        {
                            return;
                        }
                        foreach(ItemDrop drop in drops)
                        {
                            inventoryManager.AddItem(drop.item, drop.quantity);
                        }
                        if(currentlySelectedObject.GetComponent<ResourceObj>().health <= 0)
                        {
                            (int cx, int cy, int tx, int ty) c = currentlySelectedObject.GetComponent<TCactusObj>().GetCoordinates();
                            mapManager.DestroyObj(c.cx, c.cy, c.tx, c.ty);
                            currentlySelectedObject.GetComponent<TCactusObj>().Destroy();
                        }
                    }
                }
            }
            else
            {
                //Debug.Log("Nothing to click...");
            }
        }
    }

    Vector3 GetMousePosition()
    {
        
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10; // camera offset
        return mousePos;
    }
}