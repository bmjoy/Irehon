using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;

public enum ItemType { Weapon, Armor}
public enum ItemRarity {Common, Uncommon, Rare, Epic }

public class Item
{
    public int id { get; }
    public string slug { get; }
    public ItemType type { get; }
    public string name { get; }
    public string description { get; }
    public ItemRarity rarity { get; }
    public JSONNode modifiers { get; }
    public JSONNode metadata { get; }

    public Item(JSONNode json)
    {
        id = Convert.ToInt32(json["id"].Value);
        slug = json["slug"].Value;
        type = (ItemType)Enum.Parse(typeof(ItemType), json["type"].Value);
        name = json["name"].Value;
        description = json["description"].Value;
        rarity = (ItemRarity)Enum.Parse(typeof(ItemRarity), json["rarity"].Value);
        modifiers = json["modifiers"];
        metadata = json["metadata"];
    }
}

[Serializable]
public struct InventorySlot
{
    public int itemId;
    public int objectId;
    public int slotIndex;

    public InventorySlot(int slotIndex)
    {
        itemId = 0;
        objectId = 0;
        this.slotIndex = slotIndex;
    }
}

public class Inventory
{
    public const int INVENTORY_CAPACITY = 20;

    public static string GetEmptyInventoryJson()
    {
        InventorySlot[] slots = new InventorySlot[20];
        for (int i = 0; i < slots.Length; i++)
            slots[i] = new InventorySlot(i);
        return JsonHelper.ToJson(slots);
    }
}

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

        print(items[0].name);
    }
}
