using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hover : MonoBehaviour
{
    public void StartHover()
    {
        Transform upper = transform.Find("Upper");
        Transform lower = transform.Find("Lower");

        if(upper != null)
        {
            upper.gameObject.GetComponent<SpriteRenderer>().color = Color.grey;
        }
        if(lower != null)
        {
            lower.gameObject.GetComponent<SpriteRenderer>().color = Color.grey;
        }

        // must not be a tree
        if(upper == null && lower == null)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.grey;
        }
    }

    public void EndHover()
    {
        Transform upper = transform.Find("Upper");
        Transform lower = transform.Find("Lower");

        if(upper != null)
        {
            upper.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        }
        if(lower != null)
        {
            lower.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        }

        if(upper == null && lower == null)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
}
