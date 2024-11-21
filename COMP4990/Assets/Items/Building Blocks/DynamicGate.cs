using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Utils;

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
        PID = 1;
        isOpen = false;
        isHorizontal = true;
        useableTools = new ETool[]
        {
            ETool.Axe
        };
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

    Vector2[] horizontalClosed;
    Vector2[][] horizontalOpen;

    Vector2[] verticalClosed;
    Vector2[][] verticalOpen;
    
    List<Vector3Int> neighbours;

    List<int> fenceIds;

    void Start()
    {
        fenceIds = new List<int>()
        {
            101
        };

        // horizontal closed:
        horizontalClosed = new Vector2[]{
            new Vector2(0.504f, -0.343f),
            new Vector2(-0.502f, -0.338f),
            new Vector2(-0.501f, -0.472f),
            new Vector2(0.498f, -0.473f)
        };

        // horizontal open:
        horizontalOpen = new Vector2[][]{
            // Element 0
            new Vector2[]{
                new Vector2(-0.604f, -0.340f),
                new Vector2(-0.502f, -0.338f),
                new Vector2(-0.501f, -0.472f),
                new Vector2(-0.600f, -0.476f)
            },
            // Element 1
            new Vector2[]{
                new Vector2(0.500f, -0.346f),
                new Vector2(0.612f, -0.348f),
                new Vector2(0.601f, -0.473f),
                new Vector2(0.506f, -0.473f)
            }
        };

        // vertical open:
        verticalOpen = new Vector2[][]{
            // Element 0
            new Vector2[]{
                new Vector2(-0.101f, 0.556f),
                new Vector2(-0.101f, 0.538f),
                new Vector2(-0.103f, 0.316f),
                new Vector2(0.108f, 0.320f)
            },
            // Element 1
            new Vector2[]{
                new Vector2(-0.101f, -0.564f),
                new Vector2(0.099f, -0.506f),
                new Vector2(0.100f, -0.607f),
                new Vector2(-0.099f, -0.613f)
            }
        };

        // vertical closed:
        verticalClosed = new Vector2[]{
            new Vector2(0.103f, 0.570f),
            new Vector2(-0.103f, 0.574f),
            new Vector2(-0.100f, 0.344f),
            new Vector2(-0.037f, 0.277f),
            new Vector2(-0.033f, -0.301f),
            new Vector2(-0.099f, -0.371f),
            new Vector2(-0.104f, -1.209f),
            new Vector2(0.099f, -1.208f),
            new Vector2(0.098f, -0.373f),
            new Vector2(0.036f, -0.301f),
            new Vector2(0.041f, 0.276f),
            new Vector2(0.103f, 0.346f)
        };

        // I LOVE FLOOR TO INT I LOVE FLOOR TO INT I LOVE FLOOR TO INT I LOVE FLOOR TO INT I LOVE FLOOR TO INT I LOVE FLOOR TO INT
        pos = new Vector3Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z));

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
        bool hasFenceUp = IsFenceAt(up);
        bool hasFenceDown = IsFenceAt(down);

        if(hasFenceUp && hasFenceDown && isHorizontal)
        {
            isHorizontal = false;
            UpdateSurroundingBlocks();
            UpdateSprite();
        }
        else if(!hasFenceUp || !hasFenceDown && !isHorizontal)
        {
            isHorizontal = true;
            UpdateSurroundingBlocks();
            UpdateSprite();
        }

        if(!fromNeighbour)
        {
            UpdateSprite();
            UpdateSurroundingBlocks();
        }
    }

    private void UpdateSprite()
    {
        int index = 0;
        if (isOpen) index |= 0b0010;  // 2 (Open)
        if (isHorizontal) index |= 0b0001; // 1 (Horizontal)
        spriteRenderer.sprite = spriteIndex.gateSprites[index];
        UpdateColliders();
    }

    public override void Interact()
    {
        isOpen = !isOpen;
        UpdateSprite();
    }

    private bool IsFenceAt(Vector3Int position)
    {
        Placeable block = updateableBlocks.GetBlock(position);
        return block != null && fenceIds.Contains(block.id);
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

    private void UpdateSurroundingBlocks()
    {
        foreach(Vector3Int n in neighbours)
        {
            Placeable block = updateableBlocks.GetBlock(n);
            if(block != null)
            {
                block.UpdateBlock(true);
            }
        }
    }

    private void UpdateColliders()
    {
        // remake the trigger collider, this collider outlines the sprite so that it can be hovered over
        if(triggerCollider != null)
        {
            Destroy(triggerCollider);
        }
        triggerCollider = gameObject.AddComponent<PolygonCollider2D>();
        triggerCollider.isTrigger = true;

        // open and horizontal
        if(isOpen && isHorizontal)
        {
            Debug.Log("Now open and horizontal");
            nonTriggerCollider.pathCount = 2;
            nonTriggerCollider.SetPath(0, horizontalOpen[0]);
            nonTriggerCollider.SetPath(1, horizontalOpen[1]);
            nonTriggerCollider.enabled = false;
            nonTriggerCollider.enabled = true;
        }

        // closed and horizontal
        else if(!isOpen && isHorizontal)
        {
            Debug.Log("Now closed and horizontal");
            nonTriggerCollider.pathCount = 1;
            nonTriggerCollider.points = horizontalClosed;
            nonTriggerCollider.enabled = false;
            nonTriggerCollider.enabled = true;
        }

        // open and vertical
        else if(isOpen && !isHorizontal)
        {
            Debug.Log("Now open and vertical");
            nonTriggerCollider.pathCount = 2;
            nonTriggerCollider.SetPath(0, verticalOpen[0]);
            nonTriggerCollider.SetPath(1, verticalOpen[1]);
            nonTriggerCollider.enabled = false;
            nonTriggerCollider.enabled = true;
        }

        // closed and vertical
        else if(!isOpen && !isHorizontal)
        {
            Debug.Log("Now closed and vertical");
            nonTriggerCollider.pathCount = 1;
            nonTriggerCollider.points = verticalClosed;
            nonTriggerCollider.enabled = false;
            nonTriggerCollider.enabled = true;
        }
    }

    public override void Destroy()
    {
        updateableBlocks.RemoveBlock(pos);
        mapManager.SetAboveGroundTile(pos, -1);
        UpdateSurroundingBlocks();
        Destroy(gameObject);
    }
}
