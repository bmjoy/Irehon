using System.Collections.Generic;
using UnityEditor;
using SimpleJSON;
using UnityEngine;

public static class CraftDatabase
{
    private static Dictionary<int, CraftRecipe> recipes;
    public static void DatabaseLoadJson(string jsonString)
    {
        ParseCrafts(JSON.Parse(jsonString));
    }

    private static void ParseCrafts(JSONNode json)
    {
        recipes = new Dictionary<int, CraftRecipe>();
        foreach (JSONObject recipeJson in json)
        {
            CraftRecipe recipe = new CraftRecipe(recipeJson);
            recipes[recipe.id] = recipe;
        }
    }
}