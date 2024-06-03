using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float health, maxHealth = 100f;

    // transform of aim empty (child of player)
    public Transform aim;
    Vector3 mouseWorldPosition;

    public CameraController cameraController;

    public void TakeDamage(int damage)
    {
        health -= damage;
        if(health <= 0)
        {
            Debug.Log("player died lol");
        }
    }

    void RotateTowardsCursor()
    {
        mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition -= cameraController.GetOffset();
        mouseWorldPosition.z = 0;

        Vector2 dir = mouseWorldPosition - aim.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));

        aim.rotation = targetRotation;
        
        // default aim position is to the right (x = 1)
    }

    void Update()
    {
        RotateTowardsCursor();
    }

    void Start()
    {
        health = maxHealth;
    }
}
