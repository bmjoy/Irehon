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
    public event ContainerEventHandler ContainerSlotsChanged;

    public Container(JSONNode node)
    {
        this.slots = new ContainerSlot[node.Count];
        for (int i = 0; i < this.slots.Length; i++)
        {
            this.slots[i] = new ContainerSlot(this, node[i], i);
        }
    }

    public Container()
    {

    }

    public JSONNode ToJson()
    {
        JSONArray containerJson = new JSONArray();

        for (int i = 0; i < this.slots.Length; i++)
        {
            containerJson.Add(this.slots[i].ToJson());
        }

        return containerJson;
    }

    public Container(ContainerSlot[] slots)
    {
        this.slots = (ContainerSlot[])slots.Clone();
    }

    public Container(int capacity)
    {
        this.slots = new ContainerSlot[capacity];
        for (int i = 0; i < this.slots.Length; i++)
        {
            this.slots[i] = new ContainerSlot(this, i);
        }
    }

    public int GetEmptySlotsCount()
    {
        return Array.FindAll(this.slots, x => x.itemId == 0).Length;
    }

    public ContainerSlot GetEmptySlot()
    {
        return Array.Find(this.slots, x => x.itemId == 0);
    }

    public ContainerSlot[] GetEmptySlots()
    {
        return Array.FindAll(this.slots, x => x.itemId == 0);
    }

    public int GetFilledSlotsCount()
    {
        return this.GetFilledSlots().Length;
    }

    public ContainerSlot[] GetFilledSlots()
    {
        return Array.FindAll(this.slots, x => x.itemId != 0);
    }

    public bool IsEnoughSpaceForItem(int itemId, int count)
    {
        Item item = ItemDatabase.GetItemById(itemId);

        ContainerSlot[] slotsWithItem = this.FindItemSlots(itemId);
        foreach (ContainerSlot slot in slotsWithItem)
        {
            if (slot.itemQuantity != item.maxInStack)
            {
                count -= item.maxInStack - slot.itemQuantity;
            }
        }

        int requiredSlotCount = count / item.maxInStack;
        requiredSlotCount += count % item.maxInStack == 0 ? 0 : 1;

        return this.GetEmptySlotsCount() >= requiredSlotCount;
    }

    public int GetItemCount(int itemId)
    {
        int count = 0;
        Array.FindAll(this.slots, x => x.itemId == itemId).ToList().ForEach(x => count += x.itemQuantity);
        return count;
    }

    public void Truncate()
    {
        ContainerSlot[] slots = this.GetFilledSlots();
        foreach (ContainerSlot slot in slots)
        {
            slot.itemId = 0;
            slot.itemQuantity = 0;
        }

        ContainerSlotsChanged.Invoke(this);
    }

    public ContainerSlot FindItem(int itemId)
    {
        return Array.Find(this.slots, x => x.itemId == itemId);
    }

    public ContainerSlot[] FindItemSlots(int itemId)
    {
        return Array.FindAll(this.slots, x => x.itemId == itemId);
    }

    public bool Equals(Container other)
    {
        return this == other;
    }

    public ContainerSlot this[int i]
    {
        get => this.slots[i];
        set => this.slots[i] = value;
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
        {
            newContainer[i].CopyContent(filledSlots[i]);
        }

        for (int i = 0; i < containers.Count; i++)
        {
            containers[i].Truncate();
        }

        return newContainer;
    }

    public static void MoveSlotData(Container oldContainer, int oldSlotIndex, Container container, int containerSlotIndex)
    {
        ContainerSlot oldContainerSlot = oldContainer[oldSlotIndex];
        ContainerSlot containerSlot = container[containerSlotIndex];
        if (oldContainerSlot == null || containerSlot == null)
        {
            Debug.LogError("Empty slot not found");
            return;
        }

        if (containerSlot.itemId == 0 && oldContainerSlot.itemId == 0)
        {
            Debug.LogError("Empty slots error");
            return;
        }
        if (oldContainerSlot.itemId != containerSlot.itemId)
        {
            SwapSlotData(oldContainerSlot, containerSlot);
        }
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

        container.ContainerSlotsChanged?.Invoke(container);
        oldContainer.ContainerSlotsChanged?.Invoke(oldContainer);
    }

    public void SplitInEmptySlot(ContainerSlot slot, int itemCount)
    {
        if (slot == null)
        {
            Debug.LogError("Null slot error");
            return;
        }
        if (itemCount < 0 || itemCount > slot.itemQuantity)
        {

            Debug.LogError("Slot quantity error");
            return;
        }

        var emptySlot = GetEmptySlot();
        if (emptySlot == null)
            return;

        emptySlot.itemId = slot.itemId;
        emptySlot.itemQuantity = itemCount;
        slot.itemQuantity -= itemCount;
        if (slot.itemQuantity == 0)
            slot.itemId = 0;

        ContainerSlotsChanged?.Invoke(this);
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
            {
                oldContainerSlot.itemId = 0;
            }
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

    public void ClaimFromSlot(Container anotherContainer, int index, int itemCount)
    {
        if (anotherContainer == null || index < 0 || index >= anotherContainer.slots.Length)
        {
            Debug.LogError("Argument error");
            return;
        }

        if (anotherContainer.slots[index].itemId == 0)
        {
            Debug.LogError("Slot doesn't contain item");
            return;
        }

        if (itemCount <= 0 || itemCount > anotherContainer.slots[index].itemQuantity)
        {
            Debug.LogError("Item count argument error");
            return;
        }

        ContainerSlot targetSlot = anotherContainer[index];
        Item item = targetSlot.GetItem();

        var slotsWithTargetItem = FindItemSlots(targetSlot.itemId);
        if (slotsWithTargetItem.Length > 0)
        {
            //Обход и заполнение всех ячеек до фула, пока есть count
            foreach (ContainerSlot containerSlot in slotsWithTargetItem)
            {
                if (containerSlot.itemQuantity != item.maxInStack)
                {
                    if (containerSlot.itemQuantity + itemCount <= item.maxInStack)
                    {
                        containerSlot.itemQuantity += itemCount;
                        targetSlot.itemQuantity -= itemCount;
                        if (targetSlot.itemQuantity == 0)
                            targetSlot.itemId = 0;
                        Debug.Log("invoked");
                        ContainerSlotsChanged?.Invoke(this);
                        anotherContainer.ContainerSlotsChanged?.Invoke(anotherContainer);
                        return;
                    }
                    else
                    {
                        int avaliableSpace = item.maxInStack - containerSlot.itemQuantity;
                        targetSlot.itemQuantity -= avaliableSpace;
                        containerSlot.itemQuantity += avaliableSpace;
                        itemCount -= avaliableSpace;
                    }
                }
            }

            if (itemCount > 0 && GetEmptySlot() != null)
                ClaimInEmptySlots(anotherContainer, index, itemCount);
        }
        else
        {
            ClaimInEmptySlots(anotherContainer, index, itemCount);
        }

        if (targetSlot.itemQuantity == 0)
            targetSlot.itemId = 0;
        Debug.Log("invoked");
        ContainerSlotsChanged?.Invoke(this);
        anotherContainer.ContainerSlotsChanged?.Invoke(anotherContainer);
    }

    private void ClaimInEmptySlots(Container anotherContainer, int index, int itemCount)
    {
        if (anotherContainer == null || index < 0 || index >= anotherContainer.slots.Length)
        {
            Debug.LogError("Argument error");
            return;
        }

        if (anotherContainer.slots[index].itemId == 0)
        {
            Debug.LogError("Slot doesn't contain item");
            return;
        }

        if (itemCount <= 0 || itemCount > anotherContainer.slots[index].itemQuantity)
        {
            Debug.LogError("Item count argument error");
            return;
        }

        Debug.Log($"Claimning in empty slot {itemCount} items");

        ContainerSlot targetSlot = anotherContainer[index];
        Item item = targetSlot.GetItem();

        ContainerSlot emptySlot = GetEmptySlot();
        while (itemCount > 0 && emptySlot != null)
        {
            if (itemCount > item.maxInStack)
            {
                emptySlot.itemId = item.id;
                emptySlot.itemQuantity = item.maxInStack;
                targetSlot.itemQuantity -= item.maxInStack;
                itemCount -= item.maxInStack;
            }
            else
            {
                emptySlot.itemId = item.id;
                emptySlot.itemQuantity = itemCount;
                targetSlot.itemQuantity -= itemCount;
                itemCount = 0;
            }
            emptySlot = GetEmptySlot();
        }
    }

    public void DragContainerSlotData(ContainerSlot containerSlot)
    {
        if (containerSlot == null)
        {
            Debug.LogError("Empty slot not found");
            return;
        }

        if (containerSlot.itemId == 0)
        {
            return;
        }

        ContainerSlot[] slotsWithItem = this.FindItemSlots(containerSlot.itemId);
    }

    public void AddItem(int itemId, int count)
    {
        if (itemId <= 0 || count <= 0)
        {
            Debug.LogError("Argument error");
            return;
        }

        Item item = ItemDatabase.GetItemById(itemId);

        ContainerSlot[] slots = this.FindItemSlots(itemId);

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
                        ContainerSlotsChanged.Invoke(this);
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
                            ContainerSlotsChanged.Invoke(this);
                            return;
                        }
                    }
                }
            }

            //Все слоты забиты под фулл
            this.CreateItemInEmptySlot(itemId, count);
        }
        else
        {
            //Такого предмета вообще нет в инвентаре
            this.CreateItemInEmptySlot(itemId, count);
        }

        ContainerSlotsChanged.Invoke(this);
    }

    public void Sort()
    {
        this.slots.OrderByDescending(slot => slot.itemId);
        ContainerSlotsChanged.Invoke(this);
    }

    public void RemoveItemFromInventory(int itemId, int count)
    {
        foreach (ContainerSlot slot in this.slots)
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
                    ContainerSlotsChanged.Invoke(this);

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
        ContainerSlotsChanged.Invoke(this);
    }

    private void CreateItemInEmptySlot(int itemId, int count)
    {
        if (itemId <= 0 || count <= 0)
        {
            Debug.LogError("Argument error");
            return;
        }

        Item item = ItemDatabase.GetItemById(itemId);

        int requiredSlotCount = count / item.maxInStack;
        requiredSlotCount += count % item.maxInStack == 0 ? 0 : 1;

        if (this.GetEmptySlotsCount() < requiredSlotCount)
        {
            return;
        }

        int itemCounts = count;
        for (int i = 0; i < requiredSlotCount; i++)
        {
            int creatingItemCount = itemCounts > item.maxInStack ? item.maxInStack : itemCounts;
            itemCounts -= creatingItemCount;

            this.MoveObjectToEmptySlot(itemId, creatingItemCount);
        }
    }

    private void MoveObjectToEmptySlot(int itemId, int itemQuantity)
    {
        if (itemId <= 0 || itemQuantity <= 0)
        {
            Debug.LogError("Argument error");
            return;
        }

        ContainerSlot slot = this.GetEmptySlot();

        if (slot == null)
        {
            Debug.Log("Empty slot not found");
            return;
        }

        slot.itemId = itemId;
        slot.itemQuantity = itemQuantity;
    }
}