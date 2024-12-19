using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class PlayerHPTest : MonoBehaviour
{

    public int maxHealth = 100;
    public int currentHealth;
    public HealthBar healthBar;
    void Start()
    {
        currentHealth = maxHealth;
        if(healthBar)
        {
            healthBar.SetMaxHealth(maxHealth);
        }
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)){
            TakeDamage(20);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if(healthBar)
        {
            healthBar.SetHealth(currentHealth);
        }
        if(currentHealth <= 0)
        {
            Debug.Log("player died lol");
        }
    }

}

