using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Tools;

public class CombatSystem : MonoBehaviour
{
    public WeaponManager weaponManager; // should rename the script low key]
    public InventoryManager inventoryManager;
    private Item selectedItem;

    public GameObject ironLongswordPfb;

    public GameObject ironShortswordPfb;

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    void Attack()
    {
        Debug.Log("Attacked");
        selectedItem = inventoryManager.GetSelectedItem();
        if(!selectedItem.isWeapon)
        {
            return;
        }
        weaponManager.SetWeaponObject(selectedItem.weapon);
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
    
    stab animation:
    LEFT:
    position (-0.7, -0.5, 0)
    rotation (0, 0, 90)

    RIGHT:
    position (0.7, -0.5, 0)
    rotation (0, 0 -90)
    */
}
