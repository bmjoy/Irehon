using SimpleJSON;
using System.Collections.Generic;
using System.Linq;

public static class ItemDatabase
{

    private static Dictionary<int, Item> items;

    private static bool isDatabaseLoaded = false;
    private static JSONNode databaseResponse;
    public static string jsonString { get; private set; }

    public static void DatabaseLoadJson(string jsonString)
    {
        if (isDatabaseLoaded)
        {
            return;
        }

        databaseResponse = JSON.Parse(jsonString);
        isDatabaseLoaded = true;
        ItemDatabase.jsonString = jsonString;
        ParseItems();
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

    public static Item GetItemById(int id)
    {
        return items.ContainsKey(id) ? items[id] : null;
    }

    public static Item GetItemBySlug(string slug)
    {
        return items?.Values.ToList().Find(x => x.slug == slug);
    }
}
