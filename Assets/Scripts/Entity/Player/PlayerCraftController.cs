using Irehon.Interactable;
using Mirror;

public class PlayerCraftController : NetworkBehaviour
{
    private CraftRecipe[] openedRecipes;
    private Player player;
    private PlayerContainerController containerController;

    private CharacterInfo characterData => this.player.GetCharacterInfo();

    public void SendRecipeList(CraftRecipe[] recipes)
    {
        this.SendRecipesListToClient(recipes);
        this.openedRecipes = recipes;
        this.player = this.GetComponent<Player>();
        this.containerController = this.GetComponent<PlayerContainerController>();
    }

    public void CloseCrafts()
    {
        this.openedRecipes = null;
        this.CloseCraftRpc();
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
        if (index < 0 || index >= this.openedRecipes.Length)
        {
            return;
        }

        CraftRecipe recipe = this.openedRecipes[index];

        Container inventory = ContainerData.LoadedContainers[this.characterData.inventoryId];

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
            this.containerController.inventory.RemoveItemFromInventory(requirment.itemId, requirment.itemQuantity);
        }

        this.containerController.inventory.GiveContainerItem(recipe.itemId, recipe.itemQuantity);
    }
}
