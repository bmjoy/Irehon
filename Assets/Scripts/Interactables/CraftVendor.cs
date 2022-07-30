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
        }

        public override void StopInterract(Player player)
        {
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
            
        }
    }
}