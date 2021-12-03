using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftTab : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Text recipeName;

    [SerializeField]
    private Image recipeIcon;

    [SerializeField]
    private Text quantity;

    private CraftRecipe currentRecipe;
    private Item craftingItem;
    private int index;


    public void OnPointerClick(PointerEventData eventData)
    {
        CraftWindowManager.SelectRecipe(currentRecipe, index);
    }

    public void Intialize(CraftRecipe recipe, ToggleGroup group, int tabIndex)
    {
        index = tabIndex;
        currentRecipe = recipe;
        craftingItem = ItemDatabase.GetItemById(recipe.itemId);
        recipeName.text = craftingItem.name;
        quantity.text = recipe.itemQuantity > 0 ? recipe.itemQuantity.ToString() : "";
        recipeIcon.sprite = craftingItem.sprite;

        GetComponent<Toggle>().group = group;
    }
}
