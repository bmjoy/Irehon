using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon
{
    public class PlayerModelManager : SerializedMonoBehaviour
    {
        public Dictionary<string, List<GameObject>> armorPartModels = new Dictionary<string, List<GameObject>>();

        [SerializeField]
        private Dictionary<EquipmentSlot, List<GameObject>> enabledArmorParts = new Dictionary<EquipmentSlot, List<GameObject>>();
        [SerializeField]
        private Dictionary<EquipmentSlot, string> equipedArmorPartsSlug = new Dictionary<EquipmentSlot, string>();

        private void Awake()
        {
            GetComponent<PlayerContainers>().ShareEquipmentUpdated += this.UpdateEquipmentModel;
        }

        private string GetBaseSlugModel(EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.Weapon: return "hand";
                case EquipmentSlot.Gloves: return "base_gloves";
                case EquipmentSlot.Helmet: return "base_helmet";
                case EquipmentSlot.Chestplate: return "base_chestplate";
                case EquipmentSlot.Boots: return "base_boots";
                case EquipmentSlot.Leggins: return "base_leggins";
                default: return null;
            }
        }

        public void UpdateEquipmentModel(Container equipment)
        {
            for (int i = 0; i < equipment.slots.Length; i++)
            {
                if (equipment.slots[i].itemId != 0)
                {
                    Item item = ItemDatabase.GetItemById(equipment[i].itemId);
                    EquipModel(item, item.equipmentSlot);
                }
                else
                {
                    EquipModel(null, (EquipmentSlot)i);
                }
            }
        }

        public void EquipModel(Item item, EquipmentSlot slot)
        {
            if (slot == EquipmentSlot.Weapon || slot == EquipmentSlot.None)
            {
                return;
            }

            string slug;


            if (item != null && armorPartModels.ContainsKey(item.slug))
            {
                slug = item.slug;
            }
            else
            {
                slug = GetBaseSlugModel(slot);
            }

            if (equipedArmorPartsSlug.ContainsKey(slot) && equipedArmorPartsSlug[slot] == slug)
            {
                return;
            }

            equipedArmorPartsSlug[slot] = slug;

            if (enabledArmorParts.ContainsKey(slot))
            {
                foreach (GameObject model in enabledArmorParts[slot])
                {
                    model.SetActive(false);
                }

                foreach (GameObject model in armorPartModels[slug])
                {
                    model.SetActive(true);
                }

                enabledArmorParts[slot] = armorPartModels[slug];
            }
            else
            {
                foreach (GameObject model in armorPartModels[slug])
                {
                    model.SetActive(true);
                }
            }
            enabledArmorParts[slot] = armorPartModels[slug];

        }
    }
}