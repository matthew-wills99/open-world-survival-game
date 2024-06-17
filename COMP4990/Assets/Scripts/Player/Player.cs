using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public HealthBar healthBar;
    // transform of aim empty (child of player)
    public Transform aim;
    Vector3 mouseWorldPosition;

    public CameraController cameraController;
    void Start()
    {
        currentHealth = maxHealth;
        //healthBar.SetMaxHealth(maxHealth);
    }
    void Update()
    {
        RotateTowardsCursor();
        if(Input.GetKeyDown(KeyCode.Space)){
            TakeDamage(20);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
        if(currentHealth <= 0)
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


}
