using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingSystem : MonoBehaviour
{
    public MapManager mapManager;
    public CameraController cameraController;
    public Transform player;
    public InventoryManager inventoryManager;

    public Camera mainCamera;
    
    private bool buildMode = false;
    public int buildRange = 5;
    
    public TileIndex tileIndex;
    public Grid grid;
    [SerializeField] private Tilemap interactiveMap = null; // map where cursor will be drawn
    [SerializeField] private Tilemap waterTilemap = null;
    [SerializeField] private Tilemap aboveGroundTilemap = null;

    private Vector3Int previousMousePos = new Vector3Int();
    private Vector3Int mousePos;

    public float placeCooldown = 0.1f;
    private float lastPlaceTime = 0f;

    void Update()
    {
        // build mode
        if(Input.GetKeyDown(KeyCode.B))
        {
            buildMode = !buildMode;

            if(!buildMode) // if exiting build mode
            {
                interactiveMap.SetTile(mousePos, null);
            }
            if(buildMode) // if entering build mode
            {
                if(mousePos != null)
                {
                    interactiveMap.SetTile(mousePos, tileIndex.GetCursorTile());
                }
            }
            Debug.Log($"Build mode set to: {buildMode}");
        }

        if(buildMode)
        {
            BuildMode();
        }
    }

    void BuildMode()
    {
        mousePos = GetMousePosition();

        if(!mousePos.Equals(previousMousePos))
        {
            interactiveMap.SetTile(previousMousePos, null);
            interactiveMap.SetTile(mousePos, tileIndex.GetCursorTile());
            previousMousePos = mousePos;
        }

        if(Input.GetMouseButton(0))
        {
            Place();
        }

        if(Input.GetMouseButton(1)) {
            Destroy();
        }

        // cooldown
        //if(Time.time - Time.deltaTime > placingResetTime && isPlacing)
        //{
        //    isPlacing = false;
        //}
    }

    bool PlaceOffCooldown()
    {
        Debug.Log($"{Time.time} >= {lastPlaceTime} + {placeCooldown} = {lastPlaceTime + placeCooldown}");
        if(Time.time >= lastPlaceTime + placeCooldown)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool CanPlaceHere()
    {
        // tile has a tree or rock on it
        if(mapManager.TileHasObject(mousePos.x, mousePos.y))
        {
            //Debug.Log("Object");
            return false;
        }
       // distance to target too far
        if(Utils.GetDistance(mousePos.x, mousePos.y, (int)player.transform.position.x, (int)player.transform.position.y) > buildRange)
        {
            //Debug.Log("Too Far");
            return false;
        }
        // trying to place in water (will add bridges)
        if(waterTilemap.GetTile(mousePos) != null)
        {
            //Debug.Log("Water");
            return false;
        }
        // make sure no other tile already exists
        if(aboveGroundTilemap.GetTile(mousePos) == null)
        {
            return true;
        }
        return false;
    }

    void Place()
    {
        if(!PlaceOffCooldown())
        {
            Debug.Log("place on cooldown");
            return;
        }
        if(!CanPlaceHere())
        {
            return;
        }
        if(inventoryManager.GetSelectedItem().obj.GetComponent<Sapling>())
        {
            mapManager.PlaceSapling(inventoryManager.GetSelectedItem().obj, mousePos);
            inventoryManager.RemoveItem(inventoryManager.GetSelectedItem());
            lastPlaceTime = Time.time;
            return;
        }

        mapManager.SetAboveGroundTile(mousePos, 1); // temporary number
        aboveGroundTilemap.SetTile(mousePos, tileIndex.GetHouseTile());
        lastPlaceTime = Time.time;
        return;
    }

    void Destroy()
    {
        // distance to target too far
        if(Utils.GetDistance(mousePos.x, mousePos.y, (int)player.transform.position.x, (int)player.transform.position.y) > buildRange)
        {
            return;
        }
        mapManager.SetAboveGroundTile(mousePos, -1);
        aboveGroundTilemap.SetTile(mousePos, null);   
    }

    Vector3Int GetMousePosition()
    {
        
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10; // camera offset
        Vector3 worldPos;
        worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        // Convert the world position to a cell position in the tilemap
        Vector3Int tilePos = grid.WorldToCell(worldPos);

        return tilePos;
    }
}
