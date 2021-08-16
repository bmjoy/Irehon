using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModelManager : SerializedMonoBehaviour
{
    public Dictionary<string, List<GameObject>> armorPartModels = new Dictionary<string, List<GameObject>>();

    [SerializeField]
    private Dictionary<int, List<GameObject>> enabledArmorParts = new Dictionary<int, List<GameObject>>();
    [SerializeField]
    private Dictionary<int, string> equipedArmorPartsSlug = new Dictionary<int, string>();

    private void Awake()
    {
        GetComponent<Player>().OnCharacterDataUpdateEvent.AddListener(data => { });
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

    public void EquipModel(Item item, EquipmentSlot slot)
    {
        string slug;
        
        if (item != null && armorPartModels.ContainsKey(item.slug))
            slug = item.slug;
        else
            slug = GetBaseSlugModel(slot);

        if (equipedArmorPartsSlug[(int)slot] == slug)
            return;

        if (enabledArmorParts.ContainsKey((int)slot))
        {
            foreach (GameObject model in enabledArmorParts[(int)slot])
                model.SetActive(false);

            foreach (GameObject model in armorPartModels[slug])
                model.SetActive(true);
        }
    }
}
