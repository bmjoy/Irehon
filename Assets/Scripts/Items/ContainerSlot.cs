using SimpleJSON;
using System;
using UnityEngine;

public class ContainerSlot : IEquatable<ContainerSlot>
{
    public int itemId;
    public int slotIndex;
    public int itemQuantity;

    public ContainerSlot(Container parent, int slotIndex)
    {
        this.itemId = 0;
        this.itemQuantity = 0;
        this.slotIndex = slotIndex;
    }

    public ContainerSlot()
    {

    }

    public ContainerSlot(Container parent, JSONNode node)
    {
        this.itemId = node["item_id"].AsInt;
        this.slotIndex = 0;
        this.itemQuantity = node["quantity"].AsInt;
    }

    public Item GetItem()
    {
        return ItemDatabase.GetItemById(this.itemId);
    }

    public JSONObject ToJson()
    {
        JSONObject json = new JSONObject();

        json.Add("item_id", this.itemId);
        json.Add("quantity", this.itemQuantity);
        return json;
    }

    public ContainerSlot(Container parent, JSONNode node, int index)
    {
        this.itemId = node["item_id"].AsInt;
        this.slotIndex = index;
        this.itemQuantity = node["quantity"].AsInt;
    }

    public void CopyContent(ContainerSlot slot)
    {
        this.itemId = slot.itemId;
        this.itemQuantity = slot.itemQuantity;
    }

    public bool Equals(ContainerSlot other)
    {
        Debug.Log(this.itemId == other.itemId && this.slotIndex == other.slotIndex && this.itemQuantity == other.itemQuantity);
        return this.itemId == other.itemId && this.slotIndex == other.slotIndex && this.itemQuantity == other.itemQuantity;
    }
}