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
    private GameObject craftRequrimentTabPrefab;
    [SerializeField]
    private RectTransform craftTabTransform;
    [SerializeField]
    private RectTransform craftRequirmentTabTransform;
    [SerializeField]
    private UIWindow craftWindow;
    [SerializeField]
    private ToggleGroup toggleGroup;

    [SerializeField]
    private Text craftingItemName;
    [SerializeField]
    private Image craftingItemIcon;
    [SerializeField]
    private Text craftingItemAmount;
    [SerializeField]
    private Text requiredGold;

    private Player player;
    private int selectedRecipeIndex;

    private CraftRecipe selectedRecipe;
    private Container inventory;

    private List<GameObject> spawnedTabs = new List<GameObject>();
    private List<GameObject> spawnedRequirmentTabs = new List<GameObject>();


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
        if (player.GetComponent<PlayerContainerController>().Containers[ContainerType.Inventory] != null)
            i.UpdateInventory(player.GetComponent<PlayerContainerController>().Containers[ContainerType.Inventory]);

        player.GetComponent<PlayerContainerController>().OnInventoryUpdate.AddListener(x => i.UpdateInventory(x));
    }

    private void UpdateInventory(Container inventory)
    {
        this.inventory = inventory;
        if (selectedRecipe != null)
            UpdateRequirmentTab(selectedRecipe.requirment);
    }

    public static void CloseCraftWindow()
    {
        i.craftWindow.Close();
    }

    public static void ShowRecipes(CraftRecipe[] recipes)
    {
        if (recipes.Length <= 0)
            return;

        i.craftWindow.Open();

        foreach (GameObject tab in i.spawnedTabs)
            Destroy(tab);

        i.spawnedTabs.Clear();


        for (int x = 0; x < recipes.Length; x++)
        {
            GameObject tab = Instantiate(i.craftTabPrefab, i.craftTabTransform);
            tab.GetComponent<CraftTab>().Intialize(recipes[x], i.toggleGroup, x);
            i.spawnedTabs.Add(tab);
        }

        SelectRecipe(recipes[0], 0);
        i.spawnedTabs[i.selectedRecipeIndex].GetComponent<Toggle>().isOn = true;
    }

    public static void SelectRecipe(CraftRecipe recipe, int index)
    {
        i.selectedRecipe = recipe;
        i.selectedRecipeIndex = index;

        Item craftingItem = ItemDatabase.GetItemById(recipe.itemId);

        i.craftingItemName.text = craftingItem.name;
        i.craftingItemIcon.sprite = craftingItem.sprite;
        i.craftingItemAmount.text = recipe.itemQuantity > 1 ? recipe.itemQuantity.ToString() : "";
        i.requiredGold.text = recipe.goldRequirment.ToString();

        UpdateRequirmentTab(recipe.requirment);
    }

    private static void UpdateRequirmentTab(CraftRecipe.CraftRecipeRequirment[] requirments)
    {
        foreach (GameObject tab in i.spawnedRequirmentTabs)
            Destroy(tab);

        i.spawnedRequirmentTabs.Clear();

        foreach (CraftRecipe.CraftRecipeRequirment requirment in requirments)
        {
            GameObject tab = Instantiate(i.craftRequrimentTabPrefab, i.craftRequirmentTabTransform);
            tab.GetComponent<CraftRequirmentTab>().Intialize(i.inventory, requirment);
            i.spawnedRequirmentTabs.Add(tab);
        }
    }

    public void CraftSelectedRecipe()
    {
        player.GetComponent<PlayerCraftController>().Craft(selectedRecipeIndex);
    }
}
