using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SortingOrder : MonoBehaviour
{
    private SortingGroup sortingGroup;

    void Awake()
    {
        sortingGroup = GetComponent<SortingGroup>();
        sortingGroup.sortingLayerName = "above ground";
        UpdateSortingOrder();
    }

    public void UpdateSortingOrder()
    {
        sortingGroup.sortingOrder = (int)(-transform.position.y * 100);
    }
}
