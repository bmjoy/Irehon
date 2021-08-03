using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase instance;

    private List<Item> items;

    private bool isDatabaseLoaded;
    private JSONNode databaseResponse;
    public string jsonString { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this);
        else
            instance = this;
        isDatabaseLoaded = false;
    }

    public void DatabaseLoadJson(string jsonString)
    {
        if (isDatabaseLoaded)
            return;
        databaseResponse = JSON.Parse(jsonString);
        isDatabaseLoaded = true;
        ParseItems();
    }

    public void DatabaseLoad()
    {
        if (isDatabaseLoaded)
        {
            Debug.Log("Second call of loading database err");
            return;
        }
        jsonString = MySql.Database.instance.GetItemsList();
        databaseResponse = JSON.Parse(jsonString);
        isDatabaseLoaded = true;

        ParseItems();
    }

    private void ParseItems()
    {
        items = new List<Item>();
        foreach (JSONNode item in databaseResponse)
            items.Add(new Item(item));
    }

    public static Item GetItemById(int id) => instance.items?.Find(x => x.id == id);

    public static Item GetItemBySlug(string slug) => instance.items?.Find(x => x.slug == slug);
}
