using Irehon.UI;
using SimpleJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType { Weapon, Armor, Resource, Quest }
public enum ItemRarity { Poor, Common, Uncommon, Rare, Epic }
public enum EquipmentSlot { Helmet, Chestplate, Gloves, Leggins, Boots, Weapon, None }


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

    public static Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Poor: return new Color(0.62f, 0.62f, 0.62f);
            case ItemRarity.Common: return Color.white;
            case ItemRarity.Uncommon: return new Color(0.12f, 1f, 0);
            case ItemRarity.Rare: return new Color(0, 0.44f, 0.87f);
            case ItemRarity.Epic: return new Color(0.64f, 0.21f, 0.93f);
            default: return Color.white;
        }
    }

    public virtual List<TooltipMessage> GetStringMessage()
    {
        List<TooltipMessage> messages = new List<TooltipMessage>();
        messages.Add(new TooltipMessage(GetRarityColor(this.rarity), this.name, 25));

        switch (type)
        {
            case ItemType.Armor: 
                if (!metadata["ArmorResistance"].IsNull)
                {
                    messages.Add(new TooltipMessage(Color.white, $"{equipmentSlot}", 16));
                    messages.Add(new TooltipMessage(Color.white, $"{metadata["ArmorResistance"].AsFloat * 100}% Armorpart resistance", 16));
                }
                else
                {
                    Debug.LogError($"{slug} doesn't contain armor resistance");
                }
                break;
            case ItemType.Weapon:
                if (!metadata["Damage"].IsNull && !metadata["AttackSpeed"].IsNull)
                {
                    messages.Add(new TooltipMessage(Color.white, $"{type}", 16));
                    messages.Add(new TooltipMessage(Color.white, $"{metadata["Damage"].Value} Damage", 16));
                    messages.Add(new TooltipMessage(Color.white, $"{metadata["AttackSpeed"].Value} Attack speed", 16));
                }
                else
                {
                    Debug.LogError($"{slug} doesn't contain weapon attack speed or damage");
                }
                break;
            default:
                messages.Add(new TooltipMessage(Color.white, $"{type}", 16));
                break;
        }

        messages.Add(new TooltipMessage(new Color(0.8f, 0.8f, 0.8f), this.description, 17, 5));
        return messages;
    }
}