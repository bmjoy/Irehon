using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModelManager : SerializedMonoBehaviour
{
    public Dictionary<string, List<GameObject>> armorPartModels = new Dictionary<string, List<GameObject>>();

    [SerializeField]
    private Dictionary<EquipmentSlot, List<GameObject>> enabledArmorParts = new Dictionary<EquipmentSlot, List<GameObject>>();
    [SerializeField]
    private Dictionary<EquipmentSlot, string> equipedArmorPartsSlug = new Dictionary<EquipmentSlot, string>();

    private void Awake()
    {
        this.GetComponent<PlayerContainerController>().OnEquipmentUpdate += this.UpdateEquipmentModel;
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
                this.EquipModel(item, item.equipmentSlot);
            }
            else
            {
                this.EquipModel(null, (EquipmentSlot)i);
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


        if (item != null && this.armorPartModels.ContainsKey(item.slug))
        {
            slug = item.slug;
        }
        else
        {
            slug = this.GetBaseSlugModel(slot);
        }

        if (this.equipedArmorPartsSlug.ContainsKey(slot) && this.equipedArmorPartsSlug[slot] == slug)
        {
            return;
        }

        this.equipedArmorPartsSlug[slot] = slug;

        if (this.enabledArmorParts.ContainsKey(slot))
        {
            foreach (GameObject model in this.enabledArmorParts[slot])
            {
                model.SetActive(false);
            }

            foreach (GameObject model in this.armorPartModels[slug])
            {
                model.SetActive(true);
            }

            this.enabledArmorParts[slot] = this.armorPartModels[slug];
        }
        else
        {
            foreach (GameObject model in this.armorPartModels[slug])
            {
                model.SetActive(true);
            }
        }
        this.enabledArmorParts[slot] = this.armorPartModels[slug];

    }
}
