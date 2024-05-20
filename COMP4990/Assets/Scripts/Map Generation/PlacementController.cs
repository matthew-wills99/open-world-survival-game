using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlacementController : MonoBehaviour
{
    public Tilemap tilemap;
    public Tile tileToBePlaced;

    public CameraController cameraController;
    public MapController mapController;
    public TileIndex tileIndex;

    Dictionary<int, Tile> tiles;

    public bool enable;

    int selectedTile;
    Vector3 mouseWorldPosition;

    void PlaceSelectedTile()
    {
        Vector3Int cellPos = tilemap.WorldToCell(mouseWorldPosition);
        if(tilemap.GetTile(cellPos) == null)
        {
            mapController.PlaceTile(cellPos, selectedTile);

            MapController.Coords chunkPos = mapController.GetChunkCoords(cellPos.x, cellPos.y);
            //Debug.Log($"Placed tile. Tilemap coords: ({cellPos.x}, {cellPos.y}) Chunk coords: ({chunkPos.xCoord}, {chunkPos.yCoord}) Tile in chunk coords: ({cellPos.x % mapController.chunkSize},{cellPos.y % mapController.chunkSize})");
            return;
        }
    }

    void RemoveSelectedTile()
    {
        Vector3Int cellPos = tilemap.WorldToCell(mouseWorldPosition);
        if(tilemap.GetTile(cellPos) != null)
        {
            mapController.PlaceTile(cellPos, -1);
            tilemap.SetTile(cellPos, null);
        }
    }

    void Update()
    {
        mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;
        mouseWorldPosition -= cameraController.GetOffset();

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
        tiles = new Dictionary<int, Tile>();
        selectedTile = 2;
    }
}
