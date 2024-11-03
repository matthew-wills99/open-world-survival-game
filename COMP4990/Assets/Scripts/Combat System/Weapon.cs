using System.Collections;
using System.Collections.Generic;
using static Utils;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage = 15f;
    [SerializeField]
    private EAttackType attackType;

    public EAttackType GetAttackType()
    {
        return attackType;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Enemy"))
        {
            Debug.Log("Hit enemy");
            other.GetComponent<TEnemy>().TakeDamage(damage);
        }
    }
}
