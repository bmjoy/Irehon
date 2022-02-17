using Irehon.Interactable;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Irehon.UI
{
    [RequireComponent(typeof(ItemTooltip))]
    public class CraftRequirmentTab : MonoBehaviour
    {
        [SerializeField]
        private Text itemName;
        [SerializeField]
        private Text itemQuantity;
        [SerializeField]
        private Image itemIcon;
        private Item requirmentItem;

        public void Intialize(Container inventory, CraftRecipe.CraftRecipeRequirment requirment)
        {
            requirmentItem = ItemDatabase.GetItemById(requirment.itemId);
            itemName.text = requirmentItem.name;
            itemQuantity.text = $"{inventory.GetItemCount(requirmentItem.id)}/{requirment.itemQuantity}";
            itemIcon.sprite = requirmentItem.sprite;
            GetComponent<ItemTooltip>().SetItem(requirmentItem);
        }
    }
}