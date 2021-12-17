using Mirror;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public delegate void ContainerEventHandler(NetworkIdentity sender, Container container);

[Serializable]
public class Container : IEquatable<Container>
{
    public ContainerSlot[] slots;
    public delegate void ContainerEventHandler(Container container);
    public event ContainerEventHandler OnContainerUpdate;

    public Container(JSONNode node)
    {
        slots = new ContainerSlot[node.Count];
        for (int i = 0; i < slots.Length; i++)
            slots[i] = new ContainerSlot(node[i], i);
    }

    public Container()
    {

    }

    public JSONNode ToJson()
    {
        JSONArray containerJson = new JSONArray();

        for (int i = 0; i < slots.Length; i++)
            containerJson.Add(slots[i].ToJson());

        return containerJson;
    }

    public Container(ContainerSlot[] slots)
    {
        this.slots = (ContainerSlot[])slots.Clone();
    }

    public Container(int capacity)
    {
        slots = new ContainerSlot[capacity];
        for (int i = 0; i < slots.Length; i++)
            slots[i] = new ContainerSlot(i);
    }

    public int GetEmptySlotsCount() => Array.FindAll(slots, x => x.itemId == 0).Length;

    public ContainerSlot GetEmptySlot() => Array.Find(slots, x => x.itemId == 0);

    public int GetFilledSlotsCount() => GetFilledSlots().Length;

    public ContainerSlot[] GetFilledSlots() => Array.FindAll(slots, x => x.itemId != 0);

    public bool IsEnoughSpaceForItem(int itemId, int count)
    {
        Item item = ItemDatabase.GetItemById(itemId);

        var slotsWithItem = FindItemSlots(itemId);
        foreach (ContainerSlot slot in slotsWithItem)
        {
            if (slot.itemQuantity != item.maxInStack)
            {
                count -= item.maxInStack - slot.itemQuantity;
            }
        }

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
        }
    }

    public ContainerSlot FindItem(int itemId) => Array.Find(slots, x => x.itemId == itemId);

    public ContainerSlot[] FindItemSlots(int itemId) => Array.FindAll(slots, x => x.itemId == itemId);

    public bool Equals(Container other)
    {
        return this == other;
    }

    public ContainerSlot this[int i]
    {
        get => slots[i];
        set => slots[i] = value;
    }

    public static Container MoveAllItemsInNewContainer(List<Container> containers)
    {
        List<ContainerSlot> filledSlots = new List<ContainerSlot>();

        foreach (Container container in containers)
        {
            filledSlots.AddRange(container.GetFilledSlots());
        }

        int requiredSlotCount = filledSlots.Count;

        Container newContainer = new Container(requiredSlotCount);

        for (int i = 0; i < requiredSlotCount; i++)
            newContainer[i].CopyContent(filledSlots[i]);

        for (int i = 0; i < containers.Count; i++)
        {
            containers[i].Truncate();
        }

        return newContainer;
    }

    public static void MoveSlotData(ContainerSlot oldContainerSlot, ContainerSlot containerSlot)
    {
        if (oldContainerSlot == null || containerSlot == null)
        {
            Debug.LogError("Empty slot not found");
            return;
        }

        if (oldContainerSlot.itemId != containerSlot.itemId)
            SwapSlotData(oldContainerSlot, containerSlot);
        else
        {
            Item item = ItemDatabase.GetItemById(containerSlot.itemId);
            if (containerSlot.itemQuantity + oldContainerSlot.itemQuantity <= item.maxInStack)
            {
                containerSlot.itemQuantity += oldContainerSlot.itemQuantity;

                oldContainerSlot.itemId = 0;
                oldContainerSlot.itemQuantity = 0;
            }
            else
            {
                int avaliableCount = item.maxInStack - containerSlot.itemQuantity;

                containerSlot.itemQuantity += avaliableCount;

                oldContainerSlot.itemQuantity -= avaliableCount;

                if (oldContainerSlot.itemQuantity <= 0)
                {
                    oldContainerSlot.itemId = 0;
                }
            }
        }
    }

    private static void SwapSlotData(ContainerSlot oldContainerSlot, ContainerSlot containerSlot)
    {
        if (oldContainerSlot == null || containerSlot == null)
        {
            Debug.LogError("Empty slot not found");
            return;
        }

        if (containerSlot.itemId == 0)
        {
            containerSlot.itemId = oldContainerSlot.itemId;
            Item item = containerSlot.GetItem();
            containerSlot.itemQuantity = oldContainerSlot.itemQuantity > item.maxInStack ? item.maxInStack : oldContainerSlot.itemQuantity;
            oldContainerSlot.itemQuantity -= oldContainerSlot.itemQuantity > item.maxInStack ? item.maxInStack : oldContainerSlot.itemQuantity;
            if (oldContainerSlot.itemQuantity == 0)
                oldContainerSlot.itemId = 0;
        }
        else
        {
            int oldItemId = oldContainerSlot.itemId;
            int oldQuantity = oldContainerSlot.itemQuantity;

            oldContainerSlot.itemId = containerSlot.itemId;
            oldContainerSlot.itemQuantity = containerSlot.itemQuantity;

            containerSlot.itemId = oldItemId;
            containerSlot.itemQuantity = oldQuantity;
        }
    }
}


public static class ContainerExtension
{

    public static void DragContainerSlotData(this Container container, ContainerSlot containerSlot)
    {
        if (containerSlot == null)
        {
            Debug.LogError("Empty slot not found");
            return;
        }

        if (containerSlot.itemId == 0)
            return;

        var slotsWithItem = container.FindItemSlots(containerSlot.itemId);

    }

    public static void GiveContainerItem(this Container container, int itemId, int count)
    {
        if (itemId <= 0 || count <= 0)
        {
            Debug.LogError("Argument error");
            return;
        }

        Item item = ItemDatabase.GetItemById(itemId);

        ContainerSlot[] slots = container.FindItemSlots(itemId);

        //Такой предмет есть в инвентаре, и под него есть место
        if (slots.Length > 0)
        {
            int countItems = count;
            //Обход и заполнение всех ячеек до фула, пока есть count
            foreach (ContainerSlot containerSlot in slots)
            {
                if (containerSlot.itemQuantity != item.maxInStack)
                {
                    if (containerSlot.itemQuantity + countItems <= item.maxInStack)
                    {
                        containerSlot.itemQuantity += countItems;
                        return;
                    }
                    else
                    {
                        int avaliableSpace = item.maxInStack - containerSlot.itemQuantity;
                        if (countItems >= avaliableSpace)
                        {
                            countItems -= avaliableSpace;
                            containerSlot.itemQuantity += avaliableSpace;
                        }
                        else
                        {
                            containerSlot.itemQuantity += countItems;
                            countItems = 0;
                            return;
                        }
                    }
                }
            }

            //Все слоты забиты под фулл
            container.CreateItemInEmptySlot(itemId, count);
        }
        else
            //Такого предмета вообще нет в инвентаре
            container.CreateItemInEmptySlot(itemId, count);
    }

    public static void Sort(this Container container)
    {
        container.slots.OrderByDescending(slot => slot.itemId);

    }

    public static void RemoveItemFromInventory(this Container container, int itemId, int count)
    {
        foreach (ContainerSlot slot in container.slots)
        {
            if (slot.itemId == itemId)
            {
                if (slot.itemQuantity >= count)
                {
                    slot.itemQuantity -= count;

                    if (slot.itemQuantity == 0)
                    {
                        slot.itemId = 0;
                    }

                    return;
                }
                else
                {
                    count -= slot.itemQuantity;

                    slot.itemQuantity = 0;
                    slot.itemId = 0;

                    continue;
                }
            }
        }
    }

    private static void CreateItemInEmptySlot(this Container container, int itemId, int count)
    {
        if (itemId <= 0 || count <= 0)
        {
            Debug.LogError("Argument error");
            return;
        }

        Item item = ItemDatabase.GetItemById(itemId);

        int requiredSlotCount = count / item.maxInStack;
        requiredSlotCount += count % item.maxInStack == 0 ? 0 : 1;

        if (container.GetEmptySlotsCount() < requiredSlotCount)
            return;

        int itemCounts = count;
        for (int i = 0; i < requiredSlotCount; i++)
        {
            int creatingItemCount = itemCounts > item.maxInStack ? item.maxInStack : itemCounts;
            itemCounts -= creatingItemCount;

            container.MoveObjectToEmptySlot(itemId, creatingItemCount);
        }
    }

    private static void MoveObjectToEmptySlot(this Container container, int itemId, int itemQuantity)
    {
        if (itemId <= 0 || itemQuantity <= 0)
        {
            Debug.LogError("Argument error");
            return;
        }

        ContainerSlot slot = container.GetEmptySlot();

        if (slot == null)
        {
            Debug.Log("Empty slot not found");
            return;
        }

        slot.itemId = itemId;
        slot.itemQuantity = itemQuantity;
    }
}