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
    public int maxInStack { get; }
    public string slug { get; }
    public ItemType type { get; }
    public string name { get; }
    public string description { get; }
    public ItemRarity rarity { get; }
    public JSONNode modifiers { get; }
    public JSONNode metadata { get; }
    public EquipmentSlot equipmentSlot { get; }
    public Sprite sprite { get; }

    public Item(JSONNode json)
    {
        id = Convert.ToInt32(json["id"].Value);
        slug = json["slug"].Value;
        type = (ItemType)Enum.Parse(typeof(ItemType), json["type"].Value);
        name = json["name"].Value;
        maxInStack = Convert.ToInt32(json["stack"].Value);
        description = json["description"].Value;
        rarity = (ItemRarity)Enum.Parse(typeof(ItemRarity), json["rarity"].Value);
        modifiers = json["modifiers"];
        metadata = json["metadata"];
        equipmentSlot = (EquipmentSlot)Enum.Parse(typeof(EquipmentSlot), json["slot"].Value);
        sprite = Resources.Load<Sprite>("Items/" + slug);
    }

    private Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return Color.white;
            case ItemRarity.Uncommon: return Color.green;
            case ItemRarity.Rare: return Color.blue;
            case ItemRarity.Epic: return Color.magenta;
            default: return Color.white;
        }
    }

    public virtual List<TooltipMessage> GetStringMessage()
    {
        List<TooltipMessage> messages = new List<TooltipMessage>();
        messages.Add(new TooltipMessage(name, 30));
        messages.Add(new TooltipMessage(GetRarityColor(rarity), rarity.ToString(), 20));
        messages.Add(new TooltipMessage(description, 20));
        return messages;
    }
}