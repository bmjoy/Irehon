using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftRequirmentTab : MonoBehaviour
{
    [SerializeField]
    private Text itemName;
    [SerializeField]
    private Text itemQuantity;
    [SerializeField]
    private Image itemIcon;

    public void Intialize(Container inventory, CraftRecipe.CraftRecipeRequirment requirment)
    {
        Item requirmentItem = ItemDatabase.GetItemById(requirment.itemId);
        itemName.text = requirmentItem.name;
        itemQuantity.text = $"{requirment.itemQuantity}/{inventory.GetItemCount(requirmentItem.id)}";
        itemIcon.sprite = requirmentItem.sprite;
    }
}
