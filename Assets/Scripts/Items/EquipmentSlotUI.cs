using Irehon.UI;
using UnityEngine;
using UnityEngine.UI;
public class EquipmentSlotUI : InventorySlotUI
{

    [SerializeField]
    private EquipmentSlot equipmentSlotType;
    public override int slotId { get => (int)this.equipmentSlotType; protected set => base.slotId = value; }

    [SerializeField]
    private Image baseSprite;

    public override void Intialize(ContainerSlot containerSlot, Canvas canvas, ContainerType type)
    {
        this.canvas = canvas;
        this.type = ContainerType.Equipment;

        bool isSlotUpdated = true;


        if (this.itemId == containerSlot.itemId)
        {
            isSlotUpdated = false;
        }

        this.itemId = containerSlot.itemId;

        if (this.itemId == 0)
        {
            this.itemSprite.gameObject.SetActive(false);
            this.baseSprite.gameObject.SetActive(true);

            if (this.isPointerOverSlot)
            {
                TooltipWindow.HideTooltip();

                this.isPointerOverSlot = false;
            }

            if (this.isDragging)
            {
                ContainerWindowManager.i.GetDragger().gameObject.SetActive(false);
            }

            return;
        }
        else
        {
            this.itemSprite.gameObject.SetActive(true);
            this.baseSprite.gameObject.SetActive(false);
        }

        if (isSlotUpdated)
        {
            this.item = ItemDatabase.GetItemById(this.itemId);
            string slug = this.item?.slug;
            if (slug != null)
            {
                this.itemSprite.sprite = Resources.Load<Sprite>("Items/" + slug);
            }

            if (this.isDragging)
            {
                ContainerWindowManager.i.GetDraggerImage().sprite = this.itemSprite.sprite;
            }
        }
    }
}
