using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerWeaponEquipment : NetworkBehaviour
{
    private void Start()
    {
        GetComponent<PlayerContainerController>().OnEquipmentUpdate.AddListener(UpdateWeapon);
    }

    private void UpdateWeapon(Container equipment)
    {
        var weaponSlot = equipment.slots[(int)EquipmentSlot.Weapon];
        string weaponSlug;
        if (weaponSlot.itemId == 0)
        {
            weaponSlug = "fist";
        }
        else
        {
            Item weaponItem = ItemDatabase.GetItemById(weaponSlot.itemId);
            weaponSlug = weaponItem.slug;
        }
        print(weaponSlug);
        GameObject weaponPrefab = WeaponPrefabs.GetWeaponPrefab(weaponSlug);
        EquipWeapon(weaponPrefab);
    }

    private void EquipWeapon(GameObject weaponPrefab)
    {

    }
}
