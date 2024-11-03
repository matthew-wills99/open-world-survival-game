using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Weapon attack types:

melee:
    swing
    stab
*/

public class WeaponManager : MonoBehaviour
{
    private GameObject weaponObj;
    public GameObject gfx;
    private Transform gfxTransform;

    public float attackCooldown = 0.5f; // Time between attacks
    private float nextAttackTime = 0f;

    [SerializeField] private Animator anim;

    void Start()
    {
        gfxTransform = gfx.transform;
    }

    public void SetWeaponObject(GameObject newWeaponObj) {
        foreach(Transform child in gfxTransform)
        {
            Destroy(child.gameObject);
        }
        weaponObj = Instantiate(newWeaponObj, gfxTransform);

        weaponObj.GetComponent<SpriteRenderer>().sortingOrder = -1;
    }

    public void DestroyWeapon()
    {
        if(weaponObj != null)
        {
            Destroy(weaponObj);
            weaponObj = null;
            return;
        }
        Debug.LogWarning("Tried to destroy nonexistant weapon.");
    }

    public void Attack()
    {
        if(Time.time < nextAttackTime)
        {
            return;
        }
        nextAttackTime = Time.time + attackCooldown;

        weaponObj.GetComponent<TrailRenderer>().Clear();

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if(mousePos.x < transform.position.x)
        {
            anim.SetTrigger("SwingLeft");
        }
        else
        {
            anim.SetTrigger("SwingRight");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Sigma");
    }
}
