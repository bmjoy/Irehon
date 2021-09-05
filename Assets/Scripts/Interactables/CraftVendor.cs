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
        recipes = CraftDatabase.GetRecipes(recipesId);
    }
    public void Interact(Player player)
    {
        player.GetComponent<PlayerContainerController>().SendCraftList(recipes);
    }
}
