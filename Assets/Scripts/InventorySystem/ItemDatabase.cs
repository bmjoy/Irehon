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
        
        IEnumerator LoadDatabase()
        {
            var async = MySql.Database.GetItemsList();
            yield return async;
            //jsonString = async.webRequest.downloadHandler.text;
            Debug.Log(jsonString);
            databaseResponse = JSON.Parse(jsonString);
            isDatabaseLoaded = true;
            ParseItems();
        }
        //var load = LoadDatabase();
        //while (LoadDatabase().MoveNext()) ;
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
