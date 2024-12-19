using System.Collections;
using UnityEngine;

public class TEnemy : MonoBehaviour
{
    public float maxHealth = 50f;
    private float currentHealth;

    private SpriteRenderer spriteRenderer;
    public Color hitColour = Color.red;
    public float hitDuration = 0.2f;
    private Color originalColour;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColour = spriteRenderer.color;
    }

    public void TakeDamage(float damage)
    {
        StartCoroutine(HitEffect());
        currentHealth -= damage;
        Debug.Log("Enemy took " + damage + " damage.");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Destroy the target dummy when it "dies"
        Destroy(gameObject);
    }

    private IEnumerator HitEffect()
    {
        spriteRenderer.color = hitColour;
        yield return new WaitForSeconds(hitDuration);
        spriteRenderer.color = originalColour;
    }
}