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
            player.GetComponent<PlayerCraftController>().SendRecipeList(this.recipes);
        }

        public void StopInterract(Player player)
        {
            player.GetComponent<PlayerCraftController>().CloseCrafts();
        }
    }
}