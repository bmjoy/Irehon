using Mirror;
using UnityEngine;


public class PlayerWeaponEquipment : NetworkBehaviour
{
    private Weapon currentWeapon;
    private AbilitySystem abilitySystem;
    private void Awake()
    {
        this.abilitySystem = this.GetComponent<AbilitySystem>();
        this.GetComponent<PlayerContainerController>().OnEquipmentUpdate += this.UpdateWeapon;
    }

    public void UpdateWeapon(Container equipment)
    {
        ContainerSlot weaponSlot = equipment.slots[(int)EquipmentSlot.Weapon];
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
        this.EquipWeapon(weaponPrefab);
    }

    private void EquipWeapon(GameObject weaponPrefab)
    {
        if (this.currentWeapon != null)
        {
            this.currentWeapon.UnSetup(this.abilitySystem);
        }

        GameObject spawnedWeapon = Instantiate(weaponPrefab, this.transform);
        this.abilitySystem.SetWeapon(spawnedWeapon.GetComponent<Weapon>());
        this.currentWeapon = spawnedWeapon.GetComponent<Weapon>();
    }
}
