using SimpleJSON;
using System;
using System.Collections.Generic;

public enum EquipmentSlot { Helmet, Chestplate, Gloves, Leggins, Boots, Weapon, None }
public class Equipment
{
    public static readonly int EquipmentSlotLength = Enum.GetNames(typeof(EquipmentSlot)).Length;
    private Dictionary<string, int> stats;
    public Dictionary<string, int> GetStats()
    {
        return this.stats;
    }

    public int GetStat(string name)
    {
        return this.stats.ContainsKey(name) ? this.stats[name] : 0;
    }

    public void Update(Container container)
    {
        this.stats = new Dictionary<string, int>();
        for (int i = 0; i < container.slots.Length; i++)
        {
            if (container[i].itemId == 0)
            {
                return;
            }

            Item item = ItemDatabase.GetItemById(container[i].itemId);
            JSONNode itemStats = item.metadata["stats"];
            if (itemStats != null)
            {
                foreach (KeyValuePair<string, JSONNode> kvp in (JSONObject)itemStats)
                {
                    this.stats[kvp.Key] += kvp.Value.AsInt;
                }
            }
        }
    }
}
