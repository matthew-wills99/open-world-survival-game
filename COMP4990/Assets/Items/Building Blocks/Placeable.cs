using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public abstract class Placeable : MonoBehaviour
{
    public GameObject placeableParent;
    public int id;
    public abstract void UpdateBlock(bool fromNeighbour); // called every time a surrounding block is changed.

    void Awake()
    {
        placeableParent = GameObject.Find("PlaceableParent");
        transform.SetParent(placeableParent.transform);
    }
}
