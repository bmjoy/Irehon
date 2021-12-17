using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerWeaponEquipment : NetworkBehaviour
{
    private Weapon currentWeapon;
    private AbilitySystem abilitySystem;
    private void Awake()
    {
        abilitySystem = GetComponent<AbilitySystem>();
        GetComponent<PlayerContainerController>().OnEquipmentUpdate += UpdateWeapon;
    }

    public void UpdateWeapon(Container equipment)
    {
        var weaponSlot = equipment.slots[(int)EquipmentSlot.Weapon];
        string weaponSlug;
        if (weaponSlot.itemId == 0)
        {
            weaponSlug = "fists";
        }
        else
        {
            Item weaponItem = ItemDatabase.GetItemById(weaponSlot.itemId);
            weaponSlug = weaponItem.slug;
        }
        GameObject weaponPrefab = WeaponPrefabs.GetWeaponPrefab(weaponSlug);
        EquipWeapon(weaponPrefab);
    }

    private void EquipWeapon(GameObject weaponPrefab)
    {
        if (currentWeapon != null)
            currentWeapon.UnSetup(abilitySystem);
        GameObject spawnedWeapon = Instantiate(weaponPrefab, transform);
        abilitySystem.SetWeapon(spawnedWeapon.GetComponent<Weapon>());
        currentWeapon = spawnedWeapon.GetComponent<Weapon>();
    }
}
