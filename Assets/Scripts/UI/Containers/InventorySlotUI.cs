using Irehon;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Irehon.UI
{
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
        public Item item { get; protected set; }

        protected bool isDragging;
        protected bool isPointerOverSlot;
        public ContainerType type { get; protected set; }
        public virtual int slotId { get; protected set; }

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

                if (isPointerOverSlot)
                {
                    TooltipWindow.HideTooltip();

                    isPointerOverSlot = false;
                }

                if (isDragging)
                {
                    ItemDragger.Instance.GetDragger().gameObject.SetActive(false);
                }

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
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (itemId == 0)
            {
                return;
            }

            item = ItemDatabase.GetItemById(itemId);
            TooltipWindow.ShowTooltip(item.GetStringMessage());

            isPointerOverSlot = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipWindow.HideTooltip();

            isPointerOverSlot = false;
        }

        private void OnDisable()
        {
            if (isPointerOverSlot)
            {
                TooltipWindow.HideTooltip();
                isPointerOverSlot = false;
            }
            if (isDragging)
            {
                ItemDragger.Instance.GetDragger().gameObject.SetActive(false);
                isDragging = false;
            }
        }
    }
}