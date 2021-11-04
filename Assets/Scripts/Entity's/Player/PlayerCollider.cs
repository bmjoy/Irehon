using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollider : EntityCollider
{
    [SerializeField]
    private EquipmentSlot attachedSlot;
    
    private float GetDefaultModifier()
    {
        if (attachedSlot == EquipmentSlot.Helmet)
            return 1.5f;
        return 1f;
    }
    public void UpdateModifier(Container equipment)
    {
        var equipmentSlot = equipment.slots[(int)attachedSlot];

        if (equipmentSlot.itemId == 0)
        {
            damageMultiplier = GetDefaultModifier();
        }
        else
        {
            Item weaponItem = ItemDatabase.GetItemById(equipmentSlot.itemId);
            damageMultiplier = GetDefaultModifier() - weaponItem.metadata["ArmorResistance"].AsFloat;
        }
    }
}
