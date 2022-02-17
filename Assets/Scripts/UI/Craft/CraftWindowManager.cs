using Irehon;
using Irehon.Interactable;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Irehon.UI
{
    public class CraftWindowManager : MonoBehaviour
    {
        public static CraftWindowManager Instance { get; private set; }

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
        [SerializeField]
        private ItemTooltip craftingItemTooltip;
        
        private int selectedRecipeIndex;

        private CraftRecipe selectedRecipe;
        private Container inventory;

        private List<GameObject> spawnedTabs = new List<GameObject>();
        private List<GameObject> spawnedRequirmentTabs = new List<GameObject>();


        private void Awake()
        {
            Instance = this;

            Player.LocalPlayerIntialized += Intialize;
        }

        public void Intialize(Player player)
        {
            UpdateInventory(PlayerContainers.LocalInventory);

            PlayerContainers.LocalInventorUpdated += UpdateInventory;
        }

        private void UpdateInventory(Container inventory)
        {
            this.inventory = inventory;
            if (selectedRecipe != null)
            {
                UpdateRequirmentTab(selectedRecipe.requirment);
            }
        }

        public void CloseCraftWindow()
        {
            craftWindow.Close();
        }

        public void ShowRecipes(CraftRecipe[] recipes)
        {
            if (recipes.Length <= 0)
            {
                return;
            }

            craftWindow.Open();

            foreach (GameObject tab in Instance.spawnedTabs)
            {
                Destroy(tab);
            }

            spawnedTabs.Clear();


            for (int x = 0; x < recipes.Length; x++)
            {
                GameObject tab = Instantiate(craftTabPrefab, craftTabTransform);
                tab.GetComponent<CraftTab>().Intialize(recipes[x], toggleGroup, x);
                spawnedTabs.Add(tab);
            }

            SelectRecipe(recipes[0], 0);
            spawnedTabs[selectedRecipeIndex].GetComponent<Toggle>().isOn = true;
        }

        public void SelectRecipe(CraftRecipe recipe, int index)
        {
            selectedRecipe = recipe;
            selectedRecipeIndex = index;

            Item craftingItem = ItemDatabase.GetItemById(recipe.itemId);

            craftingItemName.text = craftingItem.name;
            craftingItemIcon.sprite = craftingItem.sprite;
            craftingItemAmount.text = recipe.itemQuantity > 1 ? recipe.itemQuantity.ToString() : "";
            craftingItemTooltip.SetItem(craftingItem);
            requiredGold.text = recipe.goldRequirment.ToString();

            UpdateRequirmentTab(recipe.requirment);
        }

        private void UpdateRequirmentTab(CraftRecipe.CraftRecipeRequirment[] requirments)
        {
            foreach (GameObject tab in Instance.spawnedRequirmentTabs)
            {
                Destroy(tab);
            }

            spawnedRequirmentTabs.Clear();

            foreach (CraftRecipe.CraftRecipeRequirment requirment in requirments)
            {
                GameObject tab = Instantiate(craftRequrimentTabPrefab, craftRequirmentTabTransform);
                tab.GetComponent<CraftRequirmentTab>().Intialize(inventory, requirment);
                spawnedRequirmentTabs.Add(tab);
            }
        }

        public void CraftSelectedRecipe()
        {
            if (PlayerInteracter.LocalInteractObject?.GetComponent<CraftVendor>() != null)
            {
                PlayerInteracter.LocalInteractObject.GetComponent<CraftVendor>().Craft(selectedRecipeIndex);
            }
        }

        private void OnDestroy()
        {
            PlayerContainers.LocalInventorUpdated -= UpdateInventory;
        }
    }
}