using Irehon.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    protected Image itemSprite;
    [SerializeField]
    protected Text quantityText;
    [SerializeField]
    protected Canvas canvas;
    [SerializeField]
    protected int itemId;
    protected int itemQuantity;
    protected Item item;

    protected bool isDragging;
    protected bool isPointerOverSlot;
    public ContainerType type { get; protected set; }
    public virtual int slotId { get; protected set; }

    public virtual void OnPointerClick(PointerEventData data)
    {
        if (this.itemId == 0)
        {
            return;
        }

        if (data.button == PointerEventData.InputButton.Left && Input.GetKey(KeyCode.LeftShift))
        {
            ContainerWindowManager.i.FastMoveSlot(this);
        }
        else if (data.button == PointerEventData.InputButton.Right)
        {
            ContainerWindowManager.i.UseItemSlot(this);
        }
    }

    public virtual void OnDrop(PointerEventData data)
    {
        InventorySlotUI inventorySlot = data.pointerDrag.GetComponent<InventorySlotUI>();
        if (inventorySlot == null || inventorySlot.itemId == 0)
        {
            return;
        }

        ContainerWindowManager.i.MoveSlots(inventorySlot, this);
    }

    public void OnBeginDrag(PointerEventData data)
    {
        if (this.itemId == 0)
        {
            return;
        }

        TooltipWindow.HideTooltip();
        ContainerWindowManager.i.GetDragger().gameObject.SetActive(true);
        ContainerWindowManager.i.GetDragger().position = this.GetComponent<RectTransform>().position;
        ContainerWindowManager.i.GetDraggerImage().sprite = this.itemSprite.sprite;

        this.isDragging = true;
    }

    public void OnDrag(PointerEventData data)
    {
        if (this.itemId == 0)
        {
            return;
        }

        TooltipWindow.HideTooltip();
        ContainerWindowManager.i.GetDragger().anchoredPosition += data.delta / this.canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData data)
    {
        if (this.itemId == 0)
        {
            return;
        }

        ContainerWindowManager.i.GetDragger().gameObject.SetActive(false);

        this.isDragging = false;
    }

    public virtual void Intialize(ContainerSlot containerSlot, Canvas canvas, ContainerType type)
    {
        this.canvas = canvas;
        this.slotId = containerSlot.slotIndex;
        this.type = type;

        bool isSlotUpdated = true;

        if (this.itemId == containerSlot.itemId && this.itemQuantity == containerSlot.itemQuantity)
        {
            isSlotUpdated = false;
        }

        this.itemId = containerSlot.itemId;
        this.itemQuantity = containerSlot.itemQuantity;

        if (this.itemId == 0)
        {
            this.quantityText.text = "";
            this.itemSprite.color = Color.clear;

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

        if (isSlotUpdated)
        {
            this.quantityText.text = this.itemQuantity > 1 ? this.itemQuantity.ToString() : "";
            this.itemSprite.color = Color.white;
            this.item = ItemDatabase.GetItemById(this.itemId);
            this.itemSprite.sprite = this.item.sprite;

            if (this.isDragging)
            {
                ContainerWindowManager.i.GetDraggerImage().sprite = this.itemSprite.sprite;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (this.itemId == 0)
        {
            return;
        }

        this.item = ItemDatabase.GetItemById(this.itemId);
        TooltipWindow.ShowTooltip(this.item.GetStringMessage());

        this.isPointerOverSlot = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipWindow.HideTooltip();

        this.isPointerOverSlot = false;
    }

    private void OnDisable()
    {
        if (this.isPointerOverSlot)
        {
            TooltipWindow.HideTooltip();
            this.isPointerOverSlot = false;
        }
        if (this.isDragging)
        {
            ContainerWindowManager.i.GetDragger().gameObject.SetActive(false);
            this.isDragging = false;
        }
    }
}
