using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Irehon.UI
{
    public class ItemTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Item item;
        private bool isPointerOver;

        public void SetItem(Item item)
        {
            this.item = item;
            if (isPointerOver)
            {
                if (item == null)
                    TooltipWindow.HideTooltip();
                else
                    TooltipWindow.ShowTooltip(item.GetStringMessage());
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (item == null)
            {
                return;
            }

            TooltipWindow.ShowTooltip(item.GetStringMessage());

            isPointerOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipWindow.HideTooltip();

            isPointerOver = false;
        }

        private void OnDisable()
        {
            if (isPointerOver)
            {
                TooltipWindow.HideTooltip();
                isPointerOver = false;
            }
        }
    }
}