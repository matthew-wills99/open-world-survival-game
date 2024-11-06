using UnityEngine;
using System.Collections;

public abstract class Animal : MonoBehaviour
{
    [SerializeField]
    protected float maxHealth = 50f;
    
    [SerializeField]
    protected Color hitColour = Color.red;
    [SerializeField]
    protected float hitDuration = 0.2f;

    public abstract void Hit(float damage, Transform attacker);

    public virtual void Die()
    {
        Debug.Log($"{gameObject.name} died");
        Destroy(gameObject);
    }
}