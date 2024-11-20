using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public abstract class Placeable : MonoBehaviour
{
    public GameObject placeableParent;
    public int id;
    public int PID;
    public bool isGate;
    public bool isHorizontal;
    public bool isOpen;
    public ETool[] useableTools;
    public abstract void UpdateBlock(bool fromNeighbour); // called every time a surrounding block is changed.
    public abstract void Destroy();
    public abstract void Interact();

    void Awake()
    {
        placeableParent = GameObject.Find("PlaceableParent");
        transform.SetParent(placeableParent.transform);
    }
}
