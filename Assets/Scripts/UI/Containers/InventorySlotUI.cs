using Irehon;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Irehon.UI
{
    [RequireComponent(typeof(ItemTooltip))]
    public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler, IDropHandler
    {
        public ContainerType type { get; protected set; }
        public virtual int slotId { get; protected set; }

        [SerializeField]
        protected Image itemSprite;
        [SerializeField]
        protected Text quantityText;
        [SerializeField]
        protected Canvas canvas;
        [SerializeField]
        protected int itemId;
        protected int itemQuantity;
        public Item item { get; protected set; }

        protected bool isDragging;

        protected ItemTooltip itemTooltip;
        
        public virtual void OnPointerClick(PointerEventData data)
        {
            if (itemId == 0)
            {
                return;
            }

            if (data.button == PointerEventData.InputButton.Left && Input.GetKey(KeyCode.LeftShift))
            {
                InventorySlotsManager.Instance.FastMoveSlot(this);
            }
            else if (data.button == PointerEventData.InputButton.Right)
            {
                InventorySlotsManager.Instance.UseItemSlot(this);
            }
        }

        public virtual void OnDrop(PointerEventData data)
        {
            InventorySlotUI inventorySlot = data.pointerDrag.GetComponent<InventorySlotUI>();
            if (inventorySlot == null || inventorySlot.itemId == 0)
            {
                return;
            }

            InventorySlotsManager.Instance.MoveSlots(inventorySlot, this);
        }

        public void OnBeginDrag(PointerEventData data)
        {
            if (itemId == 0)
            {
                return;
            }

            TooltipWindow.HideTooltip();
            ItemDragger.Instance.GetDragger().gameObject.SetActive(true);
            ItemDragger.Instance.GetDragger().position = GetComponent<RectTransform>().position;
            ItemDragger.Instance.GetDraggerImage().sprite = itemSprite.sprite;

            isDragging = true;
        }

        public void OnDrag(PointerEventData data)
        {
            if (itemId == 0)
            {
                return;
            }

            TooltipWindow.HideTooltip();
            ItemDragger.Instance.GetDragger().anchoredPosition += data.delta / canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData data)
        {
            if (itemId == 0)
            {
                return;
            }

            ItemDragger.Instance.GetDragger().gameObject.SetActive(false);

            isDragging = false;
        }

        public virtual void Intialize(ContainerSlot containerSlot, Canvas canvas, ContainerType type)
        {
            if (itemTooltip == null)
                itemTooltip = GetComponent<ItemTooltip>();

            this.canvas = canvas;
            this.slotId = containerSlot.slotIndex;
            this.type = type;

            bool isSlotUpdated = true;

            if (itemId == containerSlot.itemId && itemQuantity == containerSlot.itemQuantity)
            {
                isSlotUpdated = false;
            }

            itemId = containerSlot.itemId;
            itemQuantity = containerSlot.itemQuantity;

            if (itemId == 0)
            {
                item = null;
                quantityText.text = "";
                itemSprite.color = Color.clear;

                if (isDragging)
                {
                    ItemDragger.Instance.GetDragger().gameObject.SetActive(false);
                }

                itemTooltip.SetItem(item);

                return;
            }

            if (isSlotUpdated)
            {
                quantityText.text = itemQuantity > 1 ? itemQuantity.ToString() : "";
                itemSprite.color = Color.white;
                item = ItemDatabase.GetItemById(itemId);
                itemSprite.sprite = item.sprite;

                if (isDragging)
                {
                    ItemDragger.Instance.GetDraggerImage().sprite = itemSprite.sprite;
                }
            }

            itemTooltip.SetItem(item);
        }

        private void OnDisable()
        {
            if (isDragging)
            {
                ItemDragger.Instance.GetDragger().gameObject.SetActive(false);
                isDragging = false;
            }
        }
    }
}