using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class DynamicFence : Placeable
{
    public DynamicFence()
    {
        id = 101; // wooden fence
        useableTools = new ETool[]
        {
            ETool.Axe
        };
    }

    MapManager mapManager;
    public UpdateableBlocks updateableBlocks;
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

    Vector2[] pointsWhenNotTop;
    Vector2[] pointsWhenTop;
    
    List<Vector3Int> neighbours;

    List<int> fenceIds;
    List<int> gateIds;

    void Start()
    {
        fenceIds = new List<int>()
        {
            101
        };
        gateIds = new List<int>()
        {
            102
        };

        pointsWhenNotTop = new Vector2[]
        {
            new Vector2(0.189f, -0.497f),
            new Vector2(0.316f, -0.408f),
            new Vector2(0.319f, -0.252f),
            new Vector2(0.328f, 0.473f),
            new Vector2(-0.317f, 0.468f),
            new Vector2(-0.313f, 0.301f),
            new Vector2(-0.316f, -0.294f),
            new Vector2(-0.315f, -0.396f),
            new Vector2(-0.187f, -0.501f),
        };

        pointsWhenTop = new Vector2[]
        {
            new Vector2(0.189f, -0.497f),
            new Vector2(0.316f, -0.408f),
            new Vector2(0.307f, -0.292f),
            new Vector2(0.233f, -0.225f),
            new Vector2(-0.002f, -0.177f),
            new Vector2(-0.206f, -0.219f),
            new Vector2(-0.316f, -0.294f),
            new Vector2(-0.315f, -0.396f),
            new Vector2(-0.187f, -0.501f),
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
    
    public override void UpdateBlock(bool fromNeighbour)
    {
        //Debug.Log($"Updating block: {pos}");
        UpdateSprite();
        // ask me how i figured out i needed this
        if(!fromNeighbour)
        {
            UpdateSurroundingBlocks();
        }
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

    private void UpdateSprite()
    {
        bool hasFenceUp = IsFenceAt(up, false);
        bool hasFenceDown = IsFenceAt(down, false);
        bool hasFenceLeft = IsFenceAt(left, true);
        bool hasFenceRight = IsFenceAt(right, true);

        int index = 0;
        if (hasFenceDown) index |= 0b1000;   // 8 (Down)
        if (hasFenceUp) index |= 0b0100;  // 4 (Up)
        if (hasFenceLeft) index |= 0b0010;  // 2 (Left)
        if (hasFenceRight) index |= 0b0001; // 1 (Right)

        // Ensure the index is within the bounds of the array
        if (index >= 0 && index < spriteIndex.fenceSprites.Length)
        {
            spriteRenderer.sprite = spriteIndex.fenceSprites[index]; // Set the correct sprite based on the index

            UpdateColliders();
        }
    }

    private bool IsFenceAt(Vector3Int position, bool horizontal=false)
    {
        Placeable block = updateableBlocks.GetBlock(position);
        // stupid
        if(block == null)
        {
            return false;
        }

        if(horizontal && gateIds.Contains(block.id) && block.isHorizontal)
        {
            return true;
        }

        return fenceIds.Contains(block.id);
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

        // change the points of the non trigger collider depending on if there is a fence above it or not
        if(IsFenceAt(up))
        {
            // if there is a fence above it, this collider should be full size:
            nonTriggerCollider.points = pointsWhenNotTop;
        }
        else
        {
            nonTriggerCollider.points = pointsWhenTop;
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
