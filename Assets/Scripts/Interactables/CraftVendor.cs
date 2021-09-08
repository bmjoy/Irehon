using System.Collections;
using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class CraftVendor : NetworkBehaviour, IInteractable
{
    [SerializeField]
    private List<int> recipesId;

    private CraftRecipe[] recipes;

    private void Start()
    {
        StartCoroutine(GetRecipes());
    }

    private IEnumerator GetRecipes()
    {
        while (!CraftDatabase.IsLoaded)
            yield return null;

        print("Got recipes");
        recipes = CraftDatabase.GetRecipes(recipesId);
        print($"Recipes count = {recipes.Length}, 0 id = {recipes[0].itemId}");
    }
    public void Interact(Player player)
    {
        print("Interract");
        player.GetComponent<PlayerContainerController>().SendCraftList(recipes);
    }
}
