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
    private Image itemSprite;
    [SerializeField]
    TMPro.TMP_Text quantityText;
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private int itemId;
    private int itemQuantity;
    private Item item;
    public OpenedContainerType type { get; private set; }
    public int slotId { get; private set; }

    public virtual void OnPointerClick(PointerEventData data)
    {
        
    }

    public virtual void OnDrop(PointerEventData data)
    {
        InventorySlotUI inventorySlot = data.pointerDrag.GetComponent<InventorySlotUI>();
        if (inventorySlot == null || inventorySlot.itemId == 0)
            return;
        InventoryManager.instance.MoveSlots(inventorySlot, this);
    }

    public void OnBeginDrag(PointerEventData data)
    {
        if (itemId == 0)
            return;
        InventoryManager.instance.GetDragger().gameObject.SetActive(true);
        InventoryManager.instance.GetDragger().position = GetComponent<RectTransform>().position;
        InventoryManager.instance.GetDraggerImage().sprite = itemSprite.sprite;
    }

    public void OnDrag(PointerEventData data)
    {
        if (itemId == 0)
            return;
        InventoryManager.instance.GetDragger().anchoredPosition += data.delta / canvas.scaleFactor; 
    }

    public void OnEndDrag(PointerEventData data)
    {
        if (itemId == 0)
            return;
        InventoryManager.instance.GetDragger().gameObject.SetActive(false);
    }

    public void Intialize(ContainerSlot containerSlot, Canvas canvas, OpenedContainerType type)
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
            quantityText.text = itemQuantity.ToString();
            itemSprite.color = Color.white;
            item = ItemDatabase.GetItemById(itemId);
            string slug = item?.slug;
            if (slug != null)
                itemSprite.sprite = Resources.Load<Sprite>("Items/" + slug);
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
