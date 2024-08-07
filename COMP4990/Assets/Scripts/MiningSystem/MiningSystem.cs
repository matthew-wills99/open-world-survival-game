using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

public class MiningSystem : MonoBehaviour
{
    public Material highlightMaterial;

    private Material originalMaterial;
    private Transform highlight;
    private RaycastHit raycastHit;

    private bool miningMode = false;

    void Update()
    {
        // mining
        if(Input.GetKeyDown(KeyCode.M))
        {
            miningMode = !miningMode;

            if(!miningMode) // if exiting build mode
            {
                if(highlight != null)
                {
                    highlight.GetComponent<Hover>().EndHover();
                    highlight = null;
                }
            }
            Debug.Log($"Mining mode set to: {miningMode}");
        }

        if(miningMode)
        {
            MiningMode();
        }
    }

    void MiningMode()
    {
        if(highlight != null)
        {
            highlight.GetComponent<Hover>().EndHover();
            highlight = null;
        }

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(GetMousePosition());
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);
        // closest
        RaycastHit2D hit = new RaycastHit2D();
        float highestOrder = -Mathf.Infinity;

        foreach(RaycastHit2D h in hits)
        {
            if(h.collider != null && h.collider.CompareTag("Selectable"))
            {

                if(h.transform.name == "Upper" || h.transform.name == "Lower")
                {
                    float order = h.collider.transform.parent.GetComponent<SortingGroup>().sortingOrder;
                    if(order > highestOrder)
                    {
                        highestOrder = order;
                        hit = h;
                    }
                }
                else
                {
                    float order = h.collider.GetComponent<SortingGroup>().sortingOrder;
                    if(order > highestOrder)
                    {
                        highestOrder = order;
                        hit = h;
                    }
                }
            }
        }

        if (hit.collider != null)
        {
            // has selectable tag
            if(hit.collider.transform.CompareTag("Selectable"))
            {
                // if its a tree
                if(hit.transform.name == "Upper" || hit.transform.name == "Lower")
                {
                    // if it has a parent
                    if(hit.transform.parent != null)
                    {
                        // start hover on tree
                        highlight = hit.transform.parent.transform;
                        hit.transform.parent.gameObject.GetComponent<Hover>().StartHover();
                    }
                }
                // not a tree
                else
                {
                    highlight = hit.transform;
                    hit.transform.gameObject.GetComponent<Hover>().StartHover();
                }
            }
        }
    }

    Vector3 GetMousePosition()
    {
        
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10; // camera offset
        return mousePos;
    }
}