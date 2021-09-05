using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class CraftWindowManager : MonoBehaviour
{
    private static CraftWindowManager i;

    [SerializeField]
    private GameObject craftTabPrefab;
    [SerializeField]
    private RectTransform craftTabTransform;
    [SerializeField]
    private UIWindow craftWindow;
    [SerializeField]
    private ToggleGroup toggleGroup;

    [SerializeField]
    private Text recipeName;

    private Player player;

    private CraftRecipe selectedRecipe;

    private List<GameObject> spawnedTabs;

    private void Awake()
    {
        if (i != null && i != this)
            Destroy(this);
        else
            i = this;
    }

    public static void Intialize(Player player)
    {
        i.player = player;
    }

    public static void ShowRecipes(CraftRecipe[] recipes)
    {
        if (recipes.Length <= 0)
            return;

        i.craftWindow.Open();

        foreach (GameObject tab in i.spawnedTabs)
            Destroy(tab);

        i.spawnedTabs.Clear();

        foreach (CraftRecipe recipe in recipes)
        {
            GameObject tab = Instantiate(i.craftTabPrefab, i.craftTabTransform);
            tab.GetComponent<CraftTab>().Intialize(recipe, i.toggleGroup);
            i.spawnedTabs.Add(tab);
        }

        i.spawnedTabs[0].GetComponent<Toggle>().isOn = true;
        SelectRecipe(recipes[0]);
    }

    public static void SelectRecipe(CraftRecipe recipe)
    {
        i.selectedRecipe = recipe;

        Item craftingItem = ItemDatabase.GetItemById(recipe.itemId);

        i.recipeName.text = craftingItem.name;
    }
}
