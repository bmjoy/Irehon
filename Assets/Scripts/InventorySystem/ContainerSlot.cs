using System;
using SimpleJSON;

[Serializable]
public class ContainerSlot
{
    public int itemId;
    public int objectId;
    public int slotIndex;
    public int itemQuantity;

    public ContainerSlot()
    {

    }

    public ContainerSlot(int slotIndex)
    {
        itemId = 0;
        objectId = 0;
        itemQuantity = 0;
        this.slotIndex = slotIndex;
    }

    public ContainerSlot(JSONNode node)
    {
        itemId = node["item_id"].AsInt;
        objectId = node["id"].AsInt;
        itemQuantity = node["quantity"].AsInt;
    }

    public void CopyContent(ContainerSlot slot)
    {
        itemId = slot.itemId;
        objectId = slot.objectId;
        itemQuantity = slot.itemQuantity;
    }
}