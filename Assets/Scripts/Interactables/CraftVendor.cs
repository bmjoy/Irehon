using Irehon.UI;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon.Interactable
{
    public class CraftVendor : Interactable
    {
        [SerializeField]
        private List<int> recipesId;

        private CraftRecipe[] recipes;

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
        public override void Interact(Player player)
        {
            TargetSendRecipes(player.connectionToClient, recipes);
        }

        public override void StopInterract(Player player)
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

            var playerContainers = player.GetComponent<PlayerContainers>();

            if (index < 0 || index >= recipes.Length)
            {
                return;
            }

            CraftRecipe recipe = recipes[index];

            Container inventory = playerContainers.inventory;

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
                playerContainers.inventory.RemoveItemFromInventory(requirment.itemId, requirment.itemQuantity);
            }

            playerContainers.inventory.AddItem(recipe.itemId, recipe.itemQuantity);
        }
    }
}