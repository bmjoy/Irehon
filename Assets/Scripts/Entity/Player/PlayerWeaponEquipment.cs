using Mirror;
using UnityEngine;

namespace Irehon
{
    public class PlayerWeaponEquipment : NetworkBehaviour
    {
        private Weapon currentWeapon;
        private AbilitySystem abilitySystem;
        public int CurrentBlockPoints;
        public int MaxBlockPoints;
        public bool CanBlock;
        public float BlockResistance;
        private void Awake()
        {
            abilitySystem = GetComponent<AbilitySystem>();
            this.GetComponent<PlayerContainers>().ShareEquipmentUpdated += this.UpdateWeapon;
            InvokeRepeating(nameof(IncreaseBlockPoints), .2f, .2f);
        }

        public Weapon GetWeapon() => currentWeapon;

        private void IncreaseBlockPoints()
        {
            if (!CanBlock)
                return;
            if (CurrentBlockPoints < MaxBlockPoints - 10)
                CurrentBlockPoints += 10;
            else
                CurrentBlockPoints = MaxBlockPoints;
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
            Weapon weapon = weaponPrefab.GetComponent<Weapon>();
            if (weapon is MeleeWeapon)
            {
                Item weaponItem = ItemDatabase.GetItemBySlug(weapon.slug);
                CurrentBlockPoints = 0;
                MaxBlockPoints = weaponItem.metadata["BlockPoints"].AsInt;
                BlockResistance = weaponItem.metadata["BlockResistance"].AsFloat;
                CanBlock = true;
            }
            else
                CanBlock = false;
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