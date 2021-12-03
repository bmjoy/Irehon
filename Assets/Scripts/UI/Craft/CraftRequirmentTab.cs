using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


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
        requirmentItem = ItemDatabase.GetItemById(requirment.itemId);
        itemName.text = requirmentItem.name;
        itemQuantity.text = $"{inventory.GetItemCount(requirmentItem.id)}/{requirment.itemQuantity}";
        itemIcon.sprite = requirmentItem.sprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (requirmentItem == null)
            return;

        TooltipWindowController.ShowTooltip(requirmentItem.GetStringMessage());

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
    }
}
