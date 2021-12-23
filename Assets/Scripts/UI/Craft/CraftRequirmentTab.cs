using Irehon.Interactable;
using Irehon.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class CraftRequirmentTab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Text itemName;
    [SerializeField]
    private Text itemQuantity;
    [SerializeField]
    private Image itemIcon;
    private Item requirmentItem;
    private bool isPointerOverSlot;

    public void Intialize(Container inventory, CraftRecipe.CraftRecipeRequirment requirment)
    {
        this.requirmentItem = ItemDatabase.GetItemById(requirment.itemId);
        this.itemName.text = this.requirmentItem.name;
        this.itemQuantity.text = $"{inventory.GetItemCount(this.requirmentItem.id)}/{requirment.itemQuantity}";
        this.itemIcon.sprite = this.requirmentItem.sprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (this.requirmentItem == null)
        {
            return;
        }

        TooltipWindow.ShowTooltip(this.requirmentItem.GetStringMessage());

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
    }
}
