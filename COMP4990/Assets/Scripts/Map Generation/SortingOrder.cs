using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SortingOrder : MonoBehaviour
{
    private SortingGroup sortingGroup;
    private float lowestPointY = 0f;
    private float sortingScale = 100f;

    void Awake()
    {
        sortingGroup = GetComponent<SortingGroup>();
        sortingGroup.sortingLayerName = "above ground";
        UpdateSortingOrder();
    }

    public void UpdateSortingOrder()
    {
        PolygonCollider2D polygonCollider = GetComponent<PolygonCollider2D>();

        if (polygonCollider != null)
        {
            // Use the lowest vertex of the specified PolygonCollider2D
            Vector2[] points = polygonCollider.points;
            lowestPointY = float.MaxValue;
            
            foreach (Vector2 point in points)
            {
                Vector3 worldPoint = transform.TransformPoint(point);
                if (worldPoint.y < lowestPointY)
                {
                    lowestPointY = worldPoint.y;
                }
            }
        }
        else
        {
            // Fallback to the SpriteRenderer's bounding box if no collider is present
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                lowestPointY = spriteRenderer.bounds.min.y;
            }
            else
            {
                // Default to the object's transform position if neither is found
                lowestPointY = transform.position.y;
            }
        }

        // Update sorting order based on the lowest point
        sortingGroup.sortingOrder = Mathf.RoundToInt(-lowestPointY * sortingScale);
    }
}
