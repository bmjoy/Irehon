using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCraftController : NetworkBehaviour
{
    private CraftRecipe[] openedRecipes;
    private Player player;

    private CharacterInfo characterData => player.GetCharacterData();

    public void SendRecipeList(CraftRecipe[] recipes)
    {
        SendRecipesListToClient(recipes);
        openedRecipes = recipes;
        player = GetComponent<Player>();
    }

    public void CloseCrafts()
    {
        openedRecipes = null;
        CloseCraftRpc();
    }

    [TargetRpc]
    private void CloseCraftRpc()
    {
        CraftWindowManager.CloseCraftWindow();
    }

    [TargetRpc]
    private void SendRecipesListToClient(CraftRecipe[] recipes)
    {
        CraftWindowManager.ShowRecipes(recipes);
    }

    [Command]
    public void Craft(int index)
    {
        if (index < 0 || index >= openedRecipes.Length)
            return;

        StartCoroutine(Craft());
        IEnumerator Craft()
        {
            CraftRecipe recipe = openedRecipes[index];

            yield return ContainerData.LoadContainer(characterData.inventory_id);
            Container inventory = ContainerData.LoadedContainers[characterData.inventory_id];

            if (!inventory.IsEnoughSpaceForItem(recipe.itemId, recipe.itemQuantity))
                yield break;

            foreach (var requirment in recipe.requirment)
            {
                if (inventory.GetItemCount(requirment.itemId) < requirment.itemQuantity)
                    yield break;


            }
        }
    }
}
