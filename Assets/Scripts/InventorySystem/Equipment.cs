using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;

public enum EquipmentSlot { Weapon, Helmet, Chestplate, Gloves, Leggins, Boots }
public class Equipment
{
    public static readonly int EquipmentSlotLength = Enum.GetNames(typeof(EquipmentSlot)).Length;
    private Dictionary<string, int> stats;
    public Dictionary<string, int> GetStats() => stats;
    public int GetStat(string name) => stats.ContainsKey(name) ? stats[name] : 0;

    public void Update(Container container)
    {
        stats = new Dictionary<string, int>();
        for (int i = 0; i < container.slots.Length; i++)
        {
            Item item = ItemDatabase.GetItemById(container[i].itemId);
            JSONNode itemStats = item.metadata["stats"];
            if (itemStats != null)
                foreach (KeyValuePair<string, JSONNode> kvp in (JSONObject)itemStats)
                    stats[kvp.Key] += kvp.Value.AsInt;
        }
    }
}
