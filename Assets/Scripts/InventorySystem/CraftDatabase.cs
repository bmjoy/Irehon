using System.Collections.Generic;
using UnityEditor;
using SimpleJSON;
using System.Linq;
using UnityEngine;

public static class CraftDatabase
{
    private static Dictionary<int, CraftRecipe> recipes;
    public static bool IsLoaded = false;
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

        IsLoaded = true;
    }

    public static CraftRecipe GetRecipe(int id) => recipes[id];

    public static CraftRecipe[] GetRecipes(List<int> recipeId)
    {
        List<CraftRecipe> recipesList = new List<CraftRecipe>();

        foreach (int id in recipeId)
            recipesList.Add(recipes[id]);

        return recipesList.ToArray();
    }
}