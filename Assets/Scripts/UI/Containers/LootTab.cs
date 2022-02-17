using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Irehon.UI
{
    public class LootTab : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private Text itemName;
        [SerializeField]
        private Text itemType;
        [SerializeField]
        private Text itemCount;
        [SerializeField]
        private Image itemIcon;

        private ContainerSlot linkedSlot;
        public void Intialize(ContainerSlot slot)
        {
            if (slot == null || slot.itemId == 0)
            {
                Debug.LogError("Argument error, slot is empty");
                Destroy(gameObject);
                return;
            }

            linkedSlot = slot;

            Item item = slot.GetItem();
            
            itemIcon.sprite = item.sprite;
            itemName.text = item.name;
            itemType.text = item.type.ToString();
            itemCount.text = slot.itemQuantity > 1 ? slot.itemQuantity.ToString() : "";
            
            GetComponent<ItemTooltip>().SetItem(item);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                LootWindow.Instance.ClaimItem(linkedSlot.slotIndex, linkedSlot.itemQuantity);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                LootWindow.Instance.ClaimItem(linkedSlot.slotIndex, 1);
            }
        }
    }
}