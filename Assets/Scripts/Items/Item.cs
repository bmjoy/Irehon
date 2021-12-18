using SimpleJSON;
using System;
using System.Collections.Generic;
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
        this.id = Convert.ToInt32(json["id"].Value);
        this.slug = json["slug"].Value;
        this.type = (ItemType)Enum.Parse(typeof(ItemType), json["type"].Value);
        this.name = json["name"].Value;
        this.maxInStack = Convert.ToInt32(json["stack"].Value);
        this.description = json["description"].Value;
        this.rarity = (ItemRarity)Enum.Parse(typeof(ItemRarity), json["rarity"].Value);
        this.modifiers = json["modifiers"];
        this.metadata = json["metadata"];
        this.equipmentSlot = (EquipmentSlot)Enum.Parse(typeof(EquipmentSlot), json["slot"].Value);
        this.sprite = Resources.Load<Sprite>("Items/" + this.slug);
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
        messages.Add(new TooltipMessage(this.name, 30));
        messages.Add(new TooltipMessage(this.GetRarityColor(this.rarity), this.rarity.ToString(), 20));
        messages.Add(new TooltipMessage(this.description, 20, 5));
        return messages;
    }
}