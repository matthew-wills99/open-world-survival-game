using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlacementController : MonoBehaviour
{
    public Tilemap tilemap;
    public Tile tileToBePlaced;

    public bool enable;

    Tile selectedTile;
    Vector3 mouseWorldPosition;

    void PlaceSelectedTile()
    {
        Vector3Int cellPos = tilemap.WorldToCell(mouseWorldPosition);
        if(tilemap.GetTile(cellPos) == null)
        {
            tilemap.SetTile(cellPos, selectedTile);
        }
    }

    void RemoveSelectedTile()
    {
        Vector3Int cellPos = tilemap.WorldToCell(mouseWorldPosition);
        if(tilemap.GetTile(cellPos) != null)
        {
            tilemap.SetTile(cellPos, null);
        }
    }

    void Update()
    {
        mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;

        if(enable)
        {
            // left click
            if(Input.GetMouseButton(0))
            {
                PlaceSelectedTile();
            }

            // right click
            if(Input.GetMouseButton(1))
            {
                RemoveSelectedTile();
            }
        }
        
    }

    void Start()
    {
        selectedTile = tileToBePlaced;
    }
}
