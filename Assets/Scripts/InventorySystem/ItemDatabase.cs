using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;

public static class ItemDatabase
{

    private static List<Item> items;

    private static bool isDatabaseLoaded = false;
    private static JSONNode databaseResponse;
    public static string jsonString { get; private set; }

    static ItemDatabase()
    {
        if (!isDatabaseLoaded)
            DatabaseLoad();
    }

    public static void DatabaseLoadJson(string jsonString)
    {
        if (isDatabaseLoaded)
            return;
        databaseResponse = JSON.Parse(jsonString);
        isDatabaseLoaded = true;
        ParseItems();
    }

    public static void DatabaseLoad()
    {
        if (isDatabaseLoaded)
        {
            Debug.Log("Second call of loading database err");
            return;
        }
        jsonString = MySql.Database.GetItemsList();
        databaseResponse = JSON.Parse(jsonString);
        Debug.Log(jsonString);
        isDatabaseLoaded = true;

        ParseItems();
    }

    private static void ParseItems()
    {
        items = new List<Item>();
        foreach (JSONNode item in databaseResponse)
            items.Add(new Item(item));
    }

    public static Item GetItemById(int id) => items?.Find(x => x.id == id);

    public static Item GetItemBySlug(string slug) => items?.Find(x => x.slug == slug);
}
