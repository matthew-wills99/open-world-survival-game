using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingSystem : MonoBehaviour
{
    public CameraController cameraController;

    public Camera mainCamera;
    
    public TileIndex tileIndex;
    public Grid grid;
    [SerializeField] private Tilemap interactiveMap = null; // map where cursor will be drawn
    [SerializeField] private Tilemap groundTilemap = null;
    [SerializeField] private Tilemap aboveGroundTilemap = null;

    private Vector3Int previousMousePos = new Vector3Int();
    private Vector3Int mousePos;

    void Update()
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
            if(aboveGroundTilemap.GetTile(mousePos) == null)
            {
                aboveGroundTilemap.SetTile(mousePos, tileIndex.GetHouseTile());
            }
        }

        if(Input.GetMouseButton(1)) {
            aboveGroundTilemap.SetTile(mousePos, null);
        }
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
