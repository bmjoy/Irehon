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

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this);
        else
            instance = this;
        isDatabaseLoaded = false;
    }

    public void DatabaseLoad()
    {
        if (isDatabaseLoaded)
        {
            Debug.Log("Second call of loading database err");
            return;
        }
        string jsonString = MySqlServerConnection.instance.GetItemsList();
        databaseResponse = JSON.Parse(jsonString);

        ParseItems();
    }

    private void ParseItems()
    {
        items = new List<Item>();
        foreach (JSONNode item in databaseResponse)
            items.Add(new Item(item));
    }
}
