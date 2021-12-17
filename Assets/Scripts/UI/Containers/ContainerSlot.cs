using System;
using System.Collections;
using SimpleJSON;
using UnityEngine;

public class ContainerSlot : IEquatable<ContainerSlot>
{
    public int itemId;
    public int slotIndex;
    public int itemQuantity;

    public ContainerSlot(Container parent, int slotIndex)
    {
        itemId = 0;
        itemQuantity = 0;
        this.slotIndex = slotIndex;
    }

    public ContainerSlot()
    {

    }

    public ContainerSlot(Container parent, JSONNode node)
    {
        itemId = node["item_id"].AsInt;
        slotIndex = 0;
        itemQuantity = node["quantity"].AsInt;
    }

    public Item GetItem()
    {
        return ItemDatabase.GetItemById(itemId);
    }

    public JSONObject ToJson()
    {
        JSONObject json = new JSONObject();

        json.Add("item_id", itemId);
        json.Add("quantity", itemQuantity);
        return json;
    }

    public ContainerSlot(Container parent, JSONNode node, int index)
    {
        itemId = node["item_id"].AsInt;
        slotIndex = index;
        itemQuantity = node["quantity"].AsInt;
    }

    public void CopyContent(ContainerSlot slot)
    {
        itemId = slot.itemId;
        itemQuantity = slot.itemQuantity;
    }

    public bool Equals(ContainerSlot other)
    {
        Debug.Log(itemId == other.itemId && slotIndex == other.slotIndex && itemQuantity == other.itemQuantity);
        return itemId == other.itemId && slotIndex == other.slotIndex && itemQuantity == other.itemQuantity;
    }
}