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

    [SerializeField]
    private Image baseSprite;

    public override void Intialize(ContainerSlot containerSlot, Canvas canvas, ContainerType type)
    {
        this.canvas = canvas;
        this.type = ContainerType.Equipment;

        bool isSlotUpdated = true;


        if (itemId == containerSlot.itemId)
            isSlotUpdated = false;


        itemId = containerSlot.itemId;

        if (itemId == 0)
        {
            itemSprite.gameObject.SetActive(false);
            baseSprite.gameObject.SetActive(true);

            if (isPointerOverSlot)
            {
                TooltipWindowController.HideTooltip();

                isPointerOverSlot = false;
            }

            if (isDragging)
                ContainerWindowManager.i.GetDragger().gameObject.SetActive(false);

            return;
        }
        else
        {
            itemSprite.gameObject.SetActive(true);
            baseSprite.gameObject.SetActive(false);
        }

        if (isSlotUpdated)
        {
            item = ItemDatabase.GetItemById(itemId);
            string slug = item?.slug;
            if (slug != null)
                itemSprite.sprite = Resources.Load<Sprite>("Items/" + slug);

            if (isDragging)
                ContainerWindowManager.i.GetDraggerImage().sprite = itemSprite.sprite;
        }
    }
}
