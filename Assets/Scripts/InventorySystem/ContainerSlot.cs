using System;
using SimpleJSON;

[Serializable]
public class ContainerSlot
{
    public int itemId;
    public int slotIndex;
    public int itemQuantity;

    public ContainerSlot()
    {

    }

    public ContainerSlot(int slotIndex)
    {
        itemId = 0;
        itemQuantity = 0;
        this.slotIndex = slotIndex;
    }

    public ContainerSlot(JSONNode node)
    {
        itemId = node["item_id"].AsInt;
        itemQuantity = node["quantity"].AsInt;
    }

    public JSONObject ToJson()
    {
        JSONObject json = new JSONObject();

        json.Add("item_id", itemId);
        json.Add("quantity", itemQuantity);
        return json;
    }

    public ContainerSlot(JSONNode node, int index)
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
}