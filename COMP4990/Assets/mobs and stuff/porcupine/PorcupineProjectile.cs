using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PorcupineProjectile : MonoBehaviour
{
    [SerializeField]
    private float maxTravelTime = 3f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log("Hit player"); // ow
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Destroy(gameObject, maxTravelTime);
    }
}
