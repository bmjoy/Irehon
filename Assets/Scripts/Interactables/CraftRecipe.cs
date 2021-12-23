using SimpleJSON;
using System.Collections.Generic;

namespace Irehon.Interactable
{
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
                this.itemId = 0;
                this.itemQuantity = 0;
            }

            public CraftRecipeRequirment(JSONObject json)
            {
                this.itemId = json["item_id"].AsInt;
                this.itemQuantity = json["quantity"].AsInt;
            }
        }

        public CraftRecipe(JSONObject json)
        {
            this.id = json["id"].AsInt;
            this.itemId = json["craft_item"].AsInt;
            this.itemQuantity = json["craft_quantity"].AsInt;
            this.goldRequirment = json["gold_requirment"].AsInt;
            List<CraftRecipeRequirment> requirments = new List<CraftRecipeRequirment>();

            foreach (JSONObject requirmentJson in json["items_requirment"])
            {
                requirments.Add(new CraftRecipeRequirment(requirmentJson));
            }

            this.requirment = requirments.ToArray();
        }

        public CraftRecipe()
        {
        }
    }
}