using System.Collections.Generic;
using SimpleJSON;

public class CraftRecipe
{
    public int id;
    public int itemId;
    public int itemQuantity;
    public int goldRequirment;
    public CraftRecipeRequirment[] requirment;
    public class CraftRecipeRequirment
    {
        public int itemId;
        public int itemQuantity;

        public CraftRecipeRequirment()
        {
            itemId = 0;
            itemQuantity = 0;
        }

        public CraftRecipeRequirment(JSONObject json)
        {
            itemId = json["item_id"].AsInt;
            itemQuantity = json["quantity"].AsInt;
        }
    }

    public CraftRecipe(JSONObject json)
    {
        id = json["id"].AsInt;
        itemId = json["craft_item"].AsInt;
        itemQuantity = json["craft_quantity"].AsInt;
        goldRequirment = json["gold_requirment"].AsInt;
        List<CraftRecipeRequirment> requirments = new List<CraftRecipeRequirment>();

        foreach (JSONObject requirmentJson in json["items_requirment"])
            requirments.Add(new CraftRecipeRequirment(requirmentJson));

        requirment = requirments.ToArray();
    }

    public CraftRecipe()
    {
    }
}
