using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    public ContainerType type { get; protected set; }
    public virtual int slotId { get; protected set; }

    public virtual void OnPointerClick(PointerEventData data)
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            ContainerWindowManager.i.FastMoveSlot(this);
        }
    }

    public virtual void OnDrop(PointerEventData data)
    {
        InventorySlotUI inventorySlot = data.pointerDrag.GetComponent<InventorySlotUI>();
        if (inventorySlot == null || inventorySlot.itemId == 0)
            return;
        ContainerWindowManager.i.MoveSlots(inventorySlot, this);
    }

    public void OnBeginDrag(PointerEventData data)
    {
        if (itemId == 0)
            return;
        TooltipWindowController.HideTooltip();
        ContainerWindowManager.i.GetDragger().gameObject.SetActive(true);
        ContainerWindowManager.i.GetDragger().position = GetComponent<RectTransform>().position;
        ContainerWindowManager.i.GetDraggerImage().sprite = itemSprite.sprite;
    }

    public void OnDrag(PointerEventData data)
    {
        if (itemId == 0)
            return;
        TooltipWindowController.HideTooltip();
        ContainerWindowManager.i.GetDragger().anchoredPosition += data.delta / canvas.scaleFactor; 
    }

    public void OnEndDrag(PointerEventData data)
    {
        if (itemId == 0)
            return;
        ContainerWindowManager.i.GetDragger().gameObject.SetActive(false);
    }

    public virtual void Intialize(ContainerSlot containerSlot, Canvas canvas, ContainerType type)
    {
        this.canvas = canvas;
        slotId = containerSlot.slotIndex;
        this.type = type;

        bool isSlotUpdated = true;
        if (itemId == containerSlot.itemId && itemQuantity == containerSlot.itemQuantity)
            isSlotUpdated = false;
        itemId = containerSlot.itemId;
        itemQuantity = containerSlot.itemQuantity;
        if (itemId == 0)
        {
            quantityText.text = "";
            itemSprite.color = Color.clear;
            return;
        }
        if (isSlotUpdated)
        {
            quantityText.text = itemQuantity > 1 ? itemQuantity.ToString() : "";
            itemSprite.color = Color.white;
            item = ItemDatabase.GetItemById(itemId);
            itemSprite.sprite = item.sprite;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemId == 0)
            return;
        item = ItemDatabase.GetItemById(itemId);
        TooltipWindowController.ShowTooltip(item.GetStringMessage(), GetComponent<RectTransform>());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipWindowController.HideTooltip();
    }
}
