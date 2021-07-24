using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler, IDropHandler
{
    [SerializeField]
    private Image itemSprite;
    private Canvas canvas;
    private int itemId;
    public OpenedContainerType type { get; private set; }
    public int slotId { get; private set; }

    public void OnPointerClick(PointerEventData data)
    {
        //AbilityTreeController.instance.OnSelectSkill(skillId);
    }

    public void OnDrop(PointerEventData data)
    {
        InventorySlotUI inventorySlot = data.pointerDrag.GetComponent<InventorySlotUI>();
        if (inventorySlot == null || inventorySlot.itemId == 0)
            return;
        InventoryManager.instance.MoveSlots(this, inventorySlot);
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
        bool isSlotUpdated = true;
        if (itemId == containerSlot.itemId)
            isSlotUpdated = false;
        itemId = containerSlot.itemId;
        this.type = type;
        if (itemId == 0)
        {
            itemSprite.color = Color.clear;
            return;
        }
        if (isSlotUpdated)
        {
            itemSprite.color = Color.white;
            string slug = ItemDatabase.GetItemById(itemId)?.slug;
            if (slug != null)
                itemSprite.sprite = Resources.Load<Sprite>("Items/" + slug);
        }
    }
}
