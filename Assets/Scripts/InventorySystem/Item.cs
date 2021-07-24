using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public enum ItemType { Weapon, Armor, Resource, Quest }
public enum ItemRarity { Common, Uncommon, Rare, Epic }

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
    public Sprite sprite { get; }

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