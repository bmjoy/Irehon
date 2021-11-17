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
        print(inventory == null);
        print(requirment == null);
        print(requirmentItem == null);
        itemName.text = requirmentItem.name;
        itemQuantity.text = $"{inventory.GetItemCount(requirmentItem.id)}/{requirment.itemQuantity}";
        itemIcon.sprite = requirmentItem.sprite;
    }

    private void Start()
    {
        
    }
}
