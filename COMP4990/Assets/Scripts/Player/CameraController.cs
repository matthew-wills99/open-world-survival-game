using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController: MonoBehaviour
{
    //What we want to follow?
    public int zOffset = -13;
    public Transform target;
    public Vector3 offset;
    
    public Vector2 minBounds;
    public Vector2 maxBounds;

    void FixedUpdate() {
        Vector3 desiredPosition = target.position + offset;


        float clampedX = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
        float clampedY = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);

        transform.position = new Vector3(clampedX, clampedY, desiredPosition.z);
        
        //transform.LookAt(target);
    }

    public Vector3 GetOffset()
    {
        return new Vector3(offset.x, offset.y, zOffset);
    }
}
