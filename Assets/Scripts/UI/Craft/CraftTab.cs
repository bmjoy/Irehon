using Irehon.Interactable;
using UnityEngine;
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
        CraftWindowManager.Instance.SelectRecipe(this.currentRecipe, this.index);
    }

    public void Intialize(CraftRecipe recipe, ToggleGroup group, int tabIndex)
    {
        this.index = tabIndex;
        this.currentRecipe = recipe;
        this.craftingItem = ItemDatabase.GetItemById(recipe.itemId);
        this.recipeName.text = this.craftingItem.name;
        this.quantity.text = recipe.itemQuantity > 0 ? recipe.itemQuantity.ToString() : "";
        this.recipeIcon.sprite = this.craftingItem.sprite;

        this.GetComponent<Toggle>().group = group;
    }
}
