using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

/*
This class will manage the selected item in the player's hotbar
if the selected item is a tool, it will work accordingly.
if the selected item is a weapon, the correct animations will play and it will attack on clicks
if the selected item is a block or building material, it will work accordingly.

it should effectively combine miningmanager, buildingmanager, and combat manager
*/
public class PlayerActionManager : MonoBehaviour
{
    public MapManager mapManager;
    public InventoryManager inventoryManager;
    public Transform player;
    Item selectedItem;

    [Space(10)]

    // Mining ---------------------------------------------------------------------------------------------------------

    [Header("Mining")]

    private Transform highlight;
    private Transform currentlySelectedObject;

    [Space(10)]

    // Building -------------------------------------------------------------------------------------------------------

    [Header("Building")]

    public TileIndex tileIndex;
    public Grid grid;
    [SerializeField] private Tilemap interactiveMap = null; // map where cursor will be drawn
    [SerializeField] private Tilemap waterTilemap = null;
    [SerializeField] private Tilemap aboveGroundTilemap = null;

    public int buildRange = 5;
    public float placeCooldown = 0.2f;
    private float lastPlaceTime = 0f;

    private Vector3Int previouslyHoveredTile = new Vector3Int();
    private Vector3Int currentlyHoveredTile;
    private bool isCurrentlyHoveringATile = false;

    [Space(10)]

    // Combat ---------------------------------------------------------------------------------------------------------

    [Header("Combat")]

    public WeaponManager weaponManager;

    // ----------------------------------------------------------------------------------------------------------------

    int test = 0;

    void Update()
    {
        // keep track of the currently selected item
        if(inventoryManager.GetSelectedItem() != selectedItem)
        {
            selectedItem = inventoryManager.GetSelectedItem();
        }

        // reset the highlighted object
        if(highlight != null)
        {
            highlight.GetComponent<Hover>().EndHover();
            highlight = null;
        }
        else if(currentlySelectedObject != null)
        {
            currentlySelectedObject = null;
        }

        Interact();

        if(!selectedItem)
        {
            if(isCurrentlyHoveringATile)
            {
                interactiveMap.SetTile(currentlyHoveredTile, null);
                isCurrentlyHoveringATile = false;
            }
            // nothing should be happening in this script if the player does not have an item selected in their hotbar
            return;
        }

        if(!selectedItem.isPlaceable && isCurrentlyHoveringATile)
        {
            interactiveMap.SetTile(currentlyHoveredTile, null);
            isCurrentlyHoveringATile = false;
        }

        if(selectedItem.isTool)
        {
            Mining();
        }
        else if(selectedItem.isWeapon)
        {
            Combat();
        }
        else if(selectedItem.isPlaceable || selectedItem.isTile)
        {
            Building();
        }
    }

    private void Mining()
    {

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(GetMousePosition());
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);
        // closest
        RaycastHit2D hit = new RaycastHit2D();
        float highestOrder = -Mathf.Infinity;

        foreach(RaycastHit2D h in hits)
        {
            if(h.collider != null && h.collider.CompareTag("Selectable"))
            {
                // is a tree
                if(h.transform.name == "Upper" || h.transform.name == "Lower")
                {
                    float order = h.collider.transform.parent.GetComponent<SortingGroup>().sortingOrder;
                    if(order > highestOrder)
                    {
                        highestOrder = order;
                        hit = h;
                    }
                }
                // is a not tree
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
                //Debug.Log("Selectable");
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
                    //Debug.Log("she hover on my rock til i cactus");
                    highlight = hit.transform;
                    hit.transform.gameObject.GetComponent<Hover>().StartHover();
                    currentlySelectedObject = hit.transform;
                }
            }
        }

        if(Input.GetMouseButtonDown(0))
        {
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
                else if(currentlySelectedObject.GetComponent<Placeable>())
                {
                    Placeable block = currentlySelectedObject.GetComponent<Placeable>();
                    if(block.useableTools.Contains(selectedItem.toolObj.toolType))
                    {
                        // if we got it, then now we can call the break function i think
                        block.Destroy();
                    }
                }
            }
        }
    }

    private void Interact()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(GetMousePosition());
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);
        // closest
        RaycastHit2D hit = new RaycastHit2D();
        float highestOrder = -Mathf.Infinity;

        foreach(RaycastHit2D h in hits)
        {
            if(h.collider != null && h.collider.gameObject.GetComponent<Placeable>() != null)
            {
                
                float order = h.collider.GetComponent<SortingGroup>().sortingOrder;
                if(order > highestOrder)
                {
                    highestOrder = order;
                    hit = h;
                }
            }
        }

        if (hit.collider != null)
        {

            highlight = hit.transform;
            hit.transform.gameObject.GetComponent<Hover>().StartHover();
            currentlySelectedObject = hit.transform;
        }

        if(Input.GetMouseButtonDown(1))
        {
            if(currentlySelectedObject != null && currentlySelectedObject.GetComponent<Placeable>() != null)
            {
                currentlySelectedObject.GetComponent<Placeable>().Interact();
            }
        }
    }

    private void Combat()
    {
        if(Input.GetMouseButtonDown(0))
        {
            weaponManager.SetWeaponObject(selectedItem.weapon);
            weaponManager.Attack();
        }
    }

    private void Building()
    {
        currentlyHoveredTile = GetMousePositionTile();

        if(!currentlyHoveredTile.Equals(previouslyHoveredTile) || !isCurrentlyHoveringATile)
        {
            interactiveMap.SetTile(previouslyHoveredTile, null);
            interactiveMap.SetTile(currentlyHoveredTile, tileIndex.GetCursorTile());
            previouslyHoveredTile = currentlyHoveredTile;
            //Debug.Log($"CHT{test}: {currentlyHoveredTile.ToString()}");
            //test++;
        }

        if(!isCurrentlyHoveringATile)
        {
            isCurrentlyHoveringATile = true;
        }

        if(Input.GetMouseButton(0))
        {
            if(PlaceOffCooldown())
            {
                Place();
            }
        }

        if(Input.GetMouseButton(1)) {
            Destroy();
        }
    }

    private Vector3 GetMousePosition()
    {
        
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10; // camera offset
        return mousePos;
    }

    private Vector3Int GetMousePositionTile()
    {
        
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10; // camera offset
        Vector3 worldPos;
        worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        // Convert the world position to a cell position in the tilemap
        Vector3Int tilePos = grid.WorldToCell(worldPos);

        return tilePos;
    }

    void Place()
    {
        if(!CanPlaceHere())
        {
            //Debug.Log("Here");
            return;
        }
        //Debug.Log("Placing.");
        if(selectedItem.isPlaceable)
        {
            if(selectedItem.obj.GetComponent<Sapling>())
            {
                mapManager.PlaceSapling(selectedItem.obj, currentlyHoveredTile);
                inventoryManager.RemoveItem(selectedItem);
            }
            else
            {
                // maybe it will work
                // it did not work
                // IT DID WORK the bug was elsewhere
                //Debug.Log($"{test} Placing item: {currentlyHoveredTile} ttwp: {ConvertTileToWorldPos(currentlyHoveredTile)}");
                Instantiate(selectedItem.obj, ConvertTileToWorldPos(currentlyHoveredTile), Quaternion.identity);
                inventoryManager.RemoveItem(selectedItem);
                test++;
            }
        }
        else if(selectedItem.isTile)
        {
            // THIS CONDITION WILL NEVER BE SATISFIED I THINK
            //Debug.Log("hello");
            aboveGroundTilemap.SetTile(currentlyHoveredTile, selectedItem.tile);
            inventoryManager.RemoveItem(selectedItem);
            /*
            Placeable blocks (tiles) should be on a tilemap, separate from all non placeable blocks, there should be a composite collider and a tilemap collider that work
            together when you place a block to generate colliders so that you cant walk through them. however, i do not want to regenerate all box colliders on that layer,
            maybe only in that chunk? or maybe only blocks connected? something similar will need to happen when a block is destroyed.
            
            placeable objects on the other hand, (chests, workbench, anvil, etc..) will be instantiated as game objects and have their own box collider attached to it, 
            separate from the tilemap colliders
            */

            /*
            split up into tiles and objects

            tiles placed on a tilemap are not effected by sorting order script and are rendered over top of the player always
            */
        }

        lastPlaceTime = Time.time;
        return;
    }

    void Destroy()
    {
        // distance to target too far
        if(EUtils.GetDistance(
            currentlyHoveredTile.x, 
            currentlyHoveredTile.y, 
            (int)player.transform.position.x, 
            (int)player.transform.position.y
            ) > buildRange)
        {
            return;
        }
        mapManager.SetAboveGroundTile(currentlyHoveredTile, -1);
        aboveGroundTilemap.SetTile(currentlyHoveredTile, null);   
    }

    bool PlaceOffCooldown()
    {
        //Debug.Log($"{Time.time} >= {lastPlaceTime} + {placeCooldown} = {lastPlaceTime + placeCooldown}");
        if(Time.time >= lastPlaceTime + placeCooldown)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /*
    bugs somewhere here:
    sapling placed, grown, grown tree destroyed, cannot place new object on original sapling place. need to organize in mapManager or something we are so cooked
    */
    bool CanPlaceHere()
    {
        // tile has a tree or rock on it
        if(mapManager.TileHasObject(currentlyHoveredTile.x, currentlyHoveredTile.y))
        {
            //Debug.Log("Object");
            return false;
        }
        if(mapManager.TileHasPlacedTile(currentlyHoveredTile.x, currentlyHoveredTile.y))
        {
            return false;
        }
        // distance to target too far
        if(EUtils.GetDistance(
            currentlyHoveredTile.x, 
            currentlyHoveredTile.y, 
            (int)player.transform.position.x, 
            (int)player.transform.position.y
            ) > buildRange)
        {
            //Debug.Log("Too Far");
            return false;
        }
        // trying to place in water (will add bridges)
        if(waterTilemap.GetTile(currentlyHoveredTile) != null)
        {
            //Debug.Log("Water");
            return false;
        }
        // make sure no other tile already exists
        if(aboveGroundTilemap.GetTile(currentlyHoveredTile) == null)
        {
            return true;
        }
        return false;
    }

    // something
    private Vector3 ConvertTileToWorldPos(Vector3Int tilePos)
    {
        return new Vector3(tilePos.x + 0.5f, tilePos.y + 0.5f, tilePos.z);
    }
}
