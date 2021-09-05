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
    
    public void OnPointerClick(PointerEventData eventData)
    {
        CraftWindowManager.SelectRecipe(currentRecipe);
    }

    public void Intialize(CraftRecipe recipe, ToggleGroup group)
    {
        currentRecipe = recipe;
        craftingItem = ItemDatabase.GetItemById(recipe.itemId);
        recipeName.text = craftingItem.name;
        quantity.text = recipe.itemQuantity.ToString();
        recipeIcon.sprite = craftingItem.sprite;

        GetComponent<Toggle>().group = group;
    }
}
