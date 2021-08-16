using System;
using Utils;

public class Container
{
    public ContainerSlot[] slots;

    public Container(string json)
    {
        slots = JsonHelper.FromJson<ContainerSlot>(json);
    }

    public Container() {}

    public void FromJsonUpdate(string json) => slots = JsonHelper.FromJson<ContainerSlot>(json);

    public string ToJson() => JsonHelper.ToJson(slots);

    public Container(ContainerSlot[] slots)
    {
        this.slots = slots;
    }

    public Container(int capacity)
    {
        slots = new ContainerSlot[capacity];
        for (int i = 0; i < slots.Length; i++)
            slots[i] = new ContainerSlot(i);
    }

    public int GetEmptySlotsCount() => Array.FindAll(slots, x => x.objectId == 0 && x.itemId == 0).Length;

    public ContainerSlot GetEmptySlot() => Array.Find(slots, x => x.objectId == 0 && x.itemId == 0);
     
    public ContainerSlot FindObject(int objectId) => Array.Find(slots, x => x.objectId == objectId);

    public ContainerSlot FindItem(int itemId) => Array.Find(slots, x => x.itemId == itemId);

    public ContainerSlot[] FindItemSlots(int itemId) => Array.FindAll(slots, x => x.itemId == itemId);

    public ContainerSlot this[int i]
    {
        get => slots[i];
        set => slots[i] = value;
    }
}
