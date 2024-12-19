using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EUtils;

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
        Weapon weapon = weaponObj.GetComponent<Weapon>();
        if(Time.time < nextAttackTime)
        {
            return;
        }
        nextAttackTime = Time.time + weapon.attackCooldown; // cooldown of the weapon specifically

        weaponObj.GetComponent<TrailRenderer>().Clear(); // reset trail renderer of weapon

        EAttackType attackType = weapon.GetAttackType();

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(GetMousePosition());
        if(mousePos.x < transform.position.x)
        {
            // attack to left of player
            if(attackType == EAttackType.Swing)
            {
                anim.SetTrigger("SwingLeft");
            }
            else if(attackType == EAttackType.Stab)
            {
                anim.SetTrigger("StabLeft");
            }
        }
        else
        {
            // attack to right of player
            if(attackType == EAttackType.Swing)
            {
                anim.SetTrigger("SwingRight");
            }
            else if(attackType == EAttackType.Stab)
            {
                anim.SetTrigger("StabRight");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Sigma");
    }

    Vector3 GetMousePosition()
    {
        
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10; // camera offset
        return mousePos;
    }
}
