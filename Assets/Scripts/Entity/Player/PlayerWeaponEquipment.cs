using Mirror;
using UnityEngine;

namespace Irehon
{
    public class PlayerWeaponEquipment : NetworkBehaviour
    {
        private Weapon currentWeapon;
        private AbilitySystem abilitySystem;
        private void Awake()
        {
            abilitySystem = GetComponent<AbilitySystem>();
            this.GetComponent<Player>().ShareEquipmentUpdated += this.UpdateWeapon;
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
            EquipWeapon(weaponPrefab);
        }

        private void EquipWeapon(GameObject weaponPrefab)
        {
            if (currentWeapon != null)
            {
                currentWeapon.UnSetup(abilitySystem);
            }

            GameObject spawnedWeapon = Instantiate(weaponPrefab, this.transform);
            abilitySystem.SetWeapon(spawnedWeapon.GetComponent<Weapon>());
            currentWeapon = spawnedWeapon.GetComponent<Weapon>();
        }
    }
}