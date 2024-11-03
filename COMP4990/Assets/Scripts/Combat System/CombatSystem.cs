using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    public WeaponManager weaponManager; // should rename the script low key

    public GameObject ironLongswordPfb;

    void Update()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            Debug.Log("Attack");
            Attack();
        }

        if(Input.GetKeyDown(KeyCode.Y))
        {
            weaponManager.SetWeaponObject(ironLongswordPfb);
            Debug.Log("Set weapon to iron longsword");
        }
        if(Input.GetKeyDown(KeyCode.U))
        {
            weaponManager.DestroyWeapon();
            Debug.Log("Destroyed weapon");
        }
    }

    void Attack()
    {
        weaponManager.Attack();
    }

    /*

    Swing animation should be:
    LEFT:
    position (-0.45, 0.05, 0)
    rotation (0, 0, 15)
    swingrotation rot z = 71.7
    swingrotation pos x = 0.5


    RIGHT:
    position (0.45, 0.05, 0)
    rotation (0, 0, -15)
    swingrotation rot z = -71.7
    swingrotation pos x = -0.5
    
    */
}
