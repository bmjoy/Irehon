using SimpleJSON;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class Container
{
    public ContainerSlot[] slots;

    public Container(JSONNode node)
    {
        slots = new ContainerSlot[node.Count];
        for (int i = 0; i < slots.Length; i++)
            slots[i] = new ContainerSlot(node[i], i);
    }

    public Container() {}

    public JSONNode ToJson()
    {
        JSONArray containerJson = new JSONArray();

        for (int i = 0; i < slots.Length; i++)
            containerJson.Add(slots[i].ToJson());

        return containerJson;
    }

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

    public int GetFilledSlotsCount() => GetFilledSlots().Length;

    public ContainerSlot FindObject(int objectId) => Array.Find(slots, x => x.objectId == objectId);

    public ContainerSlot[] GetFilledSlots() => Array.FindAll(slots, x => x.itemId != 0);

    public bool IsEnoughSpaceForItem(int itemId, int count)
    {
        Item item = ItemDatabase.GetItemById(itemId);

        int requiredSlotCount = count / item.maxInStack;
        requiredSlotCount += count % item.maxInStack == 0 ? 0 : 1;

        return GetEmptySlotsCount() >= requiredSlotCount;
    }

    public int GetItemCount(int itemId)
    {
        int count = 0;
        Array.FindAll(slots, x => x.itemId == itemId).ToList().ForEach(x => count += x.itemQuantity);
        return count;
    }

    public void Truncate()
    {
        ContainerSlot[] slots = GetFilledSlots();
        foreach (ContainerSlot slot in slots)
        {
            slot.itemId = 0;
            slot.itemQuantity = 0;
            slot.objectId = 0;
        }
    }

    public ContainerSlot FindItem(int itemId) => Array.Find(slots, x => x.itemId == itemId);

    public ContainerSlot[] FindItemSlots(int itemId) => Array.FindAll(slots, x => x.itemId == itemId);

    public ContainerSlot this[int i]
    {
        get => slots[i];
        set => slots[i] = value;
    }
}
