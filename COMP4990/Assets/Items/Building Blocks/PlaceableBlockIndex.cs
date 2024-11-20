using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceableBlockIndex : MonoBehaviour
{
    public GameObject fencePfb;
    public GameObject gatePfb;
    public GameObject workbenchPfb;
    public GameObject forgePfb;

    Dictionary<int, GameObject> idx;

    void Awake()
    {
        idx = new Dictionary<int, GameObject>()
        {
            { 0, fencePfb},
            { 1, gatePfb},
            { 2, workbenchPfb},
            { 3, forgePfb}
        };
    }

    public GameObject GetPfbById(int id)
    {
        return idx[id];
    }
}
