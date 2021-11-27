using System.Collections;
using UnityEngine;
using Mirror;
using System.Collections.Generic;
using UnityEngine.UI;

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

        recipes = CraftDatabase.GetRecipes(recipesId);
    }
    public void Interact(Player player)
    {
        player.GetComponent<PlayerCraftController>().SendRecipeList(recipes);
    }

    public void StopInterract(Player player)
    {
        player.GetComponent<PlayerCraftController>().CloseCrafts();
    }
}
