using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicGate : Placeable
{

    /*
    dynamic gate behaviour is as follows:
    gate always places facing the camera as default
    on update block:
        if there is a fence or gate above or below, the gate will rotate to work with the vertical set of fences
        the gate will always switch to vertical regardless
        actually the priority is:
            2 fences vertically
            2 fences horizontally
            1 fence vertically
            1 fence horizontally

    keep a list of the (2 at max) neighbours that can connect to the gate
    update only those 2 neighbours, no need to check all 4 directions if the fence gate is rotated you only need to check the sides that the posts are on

    in fence:
        if there is a gate, check the boolean "horizontal" or something
        horizontal means if its left or right you update, vertical means if its above or below you update
    */

    public DynamicGate()
    {
        id = 102; // wooden gate
    }

    MapManager mapManager;
    UpdateableBlocks updateableBlocks;
    SpriteRenderer spriteRenderer;
    SpriteIndex spriteIndex;
    Vector3Int pos;

    private PolygonCollider2D triggerCollider;
    private PolygonCollider2D nonTriggerCollider;

    // coordinate of blocks up, down, left and right of this block
    Vector3Int up;
    Vector3Int down;
    Vector3Int left;
    Vector3Int right;

    Vector2[] pointsWhenHorizontalClosed;
    Vector2[] pointsWhenHorizontalOpen;

    Vector2[] pointsWhenVerticalClosed;
    Vector2[] pointsWhenVerticalOpen;
    
    List<Vector3Int> neighbours;

    // true when horizontal
    public bool isHorizontal = true;

    // true when open
    public bool isOpen = false;

    void Start()
    {
        pointsWhenHorizontalClosed = new Vector2[]
        {

        };

        // I LOVE FLOOR TO INT I LOVE FLOOR TO INT I LOVE FLOOR TO INT I LOVE FLOOR TO INT I LOVE FLOOR TO INT I LOVE FLOOR TO INT
        pos = new Vector3Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z));
        //Debug.Log($"placed a fence: {pos.x}, {pos.y}, {pos.z}");

        PolygonCollider2D[] colliders = GetComponents<PolygonCollider2D>();
        foreach(PolygonCollider2D col in colliders)
        {
            if(col.isTrigger)
            {
                triggerCollider = col;
            }
            else
            {
                nonTriggerCollider = col;
            }
        }

        mapManager = FindObjectOfType<MapManager>();
        mapManager.SetAboveGroundTile(pos, id);
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        updateableBlocks = FindObjectOfType<UpdateableBlocks>();
        spriteIndex = FindObjectOfType<SpriteIndex>();
        updateableBlocks.AddBlock(pos, this);
        InitializeNeighbours();
        UpdateBlock(false);
    }

    public override void UpdateBlock(bool fromNeighbour)
    {

    }

    public void Interact()
    {
        isOpen = !isOpen;

        int index = 0;
        if (isOpen) index |= 0b0010;  // 2 (Open)
        if (isHorizontal) index |= 0b0001; // 1 (Horizontal)

        // open horizontal = 3
        // open vertical = 2
        // closed horizontal = 1
        // closed vertical = 0

        spriteRenderer.sprite = spriteIndex.gateSprites[index];
    }

    private bool IsFenceAt(Vector3Int position)
    {
        Placeable block = updateableBlocks.GetBlock(position);
        return block != null && block.id == id;
    }

    private void InitializeNeighbours()
    {
        up = new Vector3Int(pos.x, pos.y + 1, pos.z);
        down = new Vector3Int(pos.x, pos.y - 1, pos.z);
        left = new Vector3Int(pos.x - 1, pos.y, pos.z);
        right = new Vector3Int(pos.x + 1, pos.y, pos.z);

        neighbours = new List<Vector3Int>
        {
            up,
            down,
            left,
            right
        };
    }
}
