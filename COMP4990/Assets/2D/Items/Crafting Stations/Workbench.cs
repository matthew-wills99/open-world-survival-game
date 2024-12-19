using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class Workbench : Placeable
{
    public Workbench()
    {
        PID = 3;
    }

    public override void Destroy()
    {
        throw new System.NotImplementedException();
    }

    public override void Interact()
    {
        throw new System.NotImplementedException();
    }

    public override void UpdateBlock(bool fromNeighbour)
    {
        throw new System.NotImplementedException();
    }
}