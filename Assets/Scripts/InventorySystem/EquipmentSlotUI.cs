using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class EquipmentSlotUI : InventorySlotUI
{

    [SerializeField]
    private EquipmentSlot equipmentSlotType;
    public override int slotId { get => (int)equipmentSlotType; protected set => base.slotId = value; }
    private Sprite baseSprite;

    private void Start()
    {
        baseSprite = itemSprite.sprite;
    }

    public override void Intialize(ContainerSlot containerSlot, Canvas canvas, OpenedContainerType type)
    {
        this.canvas = canvas;
        this.type = OpenedContainerType.Equipment;

        bool isSlotUpdated = true;
        if (itemId == containerSlot.itemId)
            isSlotUpdated = false;
        itemId = containerSlot.itemId;
        if (itemId == 0)
        {
            itemSprite.sprite = baseSprite;
            return;
        }
        if (isSlotUpdated)
        {
            item = ItemDatabase.GetItemById(itemId);
            string slug = item?.slug;
            if (slug != null)
                itemSprite.sprite = Resources.Load<Sprite>("Items/" + slug);
        }
    }
}
