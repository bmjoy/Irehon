using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.Linq;
using System;

public static class ItemDatabase
{

    private static Dictionary<int, Item> items;

    private static bool isDatabaseLoaded = false;
    private static JSONNode databaseResponse;
    public static string jsonString { get; private set; }

    public static void DatabaseLoadJson(string jsonString)
    {
        if (isDatabaseLoaded)
            return;
        databaseResponse = JSON.Parse(jsonString);
        isDatabaseLoaded = true;
        ItemDatabase.jsonString = jsonString;
        Debug.Log(jsonString);
        ParseItems();
    }

    private static void ParseItems()
    {
        items = new Dictionary<int, Item>();
        foreach (JSONNode item in databaseResponse)
        {
            Item newItem = new Item(item);
            items[newItem.id] = newItem;
        }
    }

    public static Item GetItemById(int id) => items[id];

    public static Item GetItemBySlug(string slug) => items?.Values.ToList().Find(x => x.slug == slug);
}
