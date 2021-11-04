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

            yield return ContainerData.LoadContainer(characterData.inventoryId);
            Container inventory = ContainerData.LoadedContainers[characterData.inventoryId];

            if (!inventory.IsEnoughSpaceForItem(recipe.itemId, recipe.itemQuantity))
                yield break;

            foreach (var requirment in recipe.requirment)
            {
                if (inventory.GetItemCount(requirment.itemId) < requirment.itemQuantity)
                    yield break;
            }

            foreach (var requirment in recipe.requirment)
            {
                yield return ContainerData.RemoveItemsFromInventory(characterData.inventoryId, requirment.itemId, requirment.itemQuantity);
            }

            yield return ContainerData.GiveContainerItem(characterData.inventoryId, recipe.itemId, recipe.itemQuantity);
        }
    }
}
