using System;

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
}