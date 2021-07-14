using System;

[Serializable]
public struct InventorySlot
{
    public int itemId;
    public int objectId;
    public int slotIndex;

    public InventorySlot(int slotIndex)
    {
        itemId = 0;
        objectId = 0;
        this.slotIndex = slotIndex;
    }
}