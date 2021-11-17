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

    protected bool isDragging;
    protected bool isPointerOverSlot;
    public ContainerType type { get; protected set; }
    public virtual int slotId { get; protected set; }

    public virtual void OnPointerClick(PointerEventData data)
    {
        if (itemId == 0)
            return;

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

        isDragging = true;
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

        isDragging = false;
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

            if (isPointerOverSlot)
            {
                TooltipWindowController.HideTooltip();

                isPointerOverSlot = false;
            }

            if (isDragging)
                ContainerWindowManager.i.GetDragger().gameObject.SetActive(false);
            
            return;
        }

        if (isSlotUpdated)
        {
            quantityText.text = itemQuantity > 1 ? itemQuantity.ToString() : "";
            itemSprite.color = Color.white;
            item = ItemDatabase.GetItemById(itemId);
            itemSprite.sprite = item.sprite;

            if (isDragging)
                ContainerWindowManager.i.GetDraggerImage().sprite = itemSprite.sprite;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemId == 0)
            return;

        item = ItemDatabase.GetItemById(itemId);
        TooltipWindowController.ShowTooltip(item.GetStringMessage(), GetComponent<RectTransform>());

        isPointerOverSlot = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipWindowController.HideTooltip();

        isPointerOverSlot = false;
    }

    private void OnDisable()
    {
        if (isPointerOverSlot)
        {
            TooltipWindowController.HideTooltip();
            isPointerOverSlot = false;
        }
        if (isDragging)
        {
            ContainerWindowManager.i.GetDragger().gameObject.SetActive(false);
            isDragging = false;
        }
    }
}
