using Irehon.Entitys;
using UnityEngine;

public class PlayerCollider : EntityCollider
{
    [SerializeField]
    private EquipmentSlot attachedSlot;

    private float GetDefaultModifier()
    {
        if (this.attachedSlot == EquipmentSlot.Helmet)
        {
            return 1.5f;
        }

        return 1f;
    }
    public void UpdateModifier(Container equipment)
    {
        ContainerSlot equipmentSlot = equipment.slots[(int)this.attachedSlot];

        if (equipmentSlot.itemId == 0)
        {
            this.damageMultiplier = this.GetDefaultModifier();
        }
        else
        {
            Item equipmentItem = ItemDatabase.GetItemById(equipmentSlot.itemId);
            this.damageMultiplier = this.GetDefaultModifier() - equipmentItem.metadata["ArmorResistance"].AsFloat;
        }
    }
}
