using Irehon.UI;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon.Interactable
{
    public class CraftVendor : NetworkBehaviour, IInteractable
    {
        [SerializeField]
        private List<int> recipesId;

        private CraftRecipe[] recipes;

        private void Awake()
        {
            this.gameObject.layer = 12;
        }

        private void Start()
        {
            this.StartCoroutine(this.GetRecipes());
        }

        private IEnumerator GetRecipes()
        {
            while (!CraftDatabase.IsLoaded)
            {
                yield return null;
            }

            this.recipes = CraftDatabase.GetRecipes(this.recipesId);
        }
        public void Interact(Player player)
        {
            TargetSendRecipes(player.connectionToClient, recipes);
        }

        public void StopInterract(Player player)
        {
            TargetCloseCraftWindow(player.connectionToClient);
        }


        [TargetRpc]
        private void TargetCloseCraftWindow(NetworkConnection target)
        {
            CraftWindowManager.Instance.CloseCraftWindow();
        }

        [TargetRpc]
        private void TargetSendRecipes(NetworkConnection target, CraftRecipe[] recipes)
        {
            CraftWindowManager.Instance.ShowRecipes(recipes);
        }

        [Command(requiresAuthority = false)]
        public void Craft(int index, NetworkConnectionToClient sender = null)
        {
            Player player = sender.identity.gameObject.GetComponent<Player>();

            if (player.GetComponent<PlayerInteracter>().currentInteractable != this)
                return;

            if (index < 0 || index >= recipes.Length)
            {
                return;
            }

            CraftRecipe recipe = recipes[index];

            Container inventory = player.inventory;

            if (!inventory.IsEnoughSpaceForItem(recipe.itemId, recipe.itemQuantity))
            {
                return;
            }

            foreach (CraftRecipe.CraftRecipeRequirment requirment in recipe.requirment)
            {
                if (inventory.GetItemCount(requirment.itemId) < requirment.itemQuantity)
                {
                    return;
                }
            }

            foreach (CraftRecipe.CraftRecipeRequirment requirment in recipe.requirment)
            {
                player.inventory.RemoveItemFromInventory(requirment.itemId, requirment.itemQuantity);
            }

            player.inventory.AddItem(recipe.itemId, recipe.itemQuantity);
        }
    }
}