using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class PairValue<T, A>
{
    public T FirstValue;
    public A SecondValue;
    public PairValue(T First, A Second)
    {
        FirstValue = First;
        SecondValue = Second;
    }
}

public static class ContainerData
{
    public static class ContainerUpdateNotifier
    {
        private static Dictionary<int, List< Action<int, Container> > > subscribedListeners = new Dictionary<int, List<Action<int, Container>>>();

        public static void Notify(int containerId)
        {
            List < Action<int, Container> > subscribers = subscribedListeners[containerId];

            foreach (var subscriber in subscribers)
                subscriber(containerId, LoadedContainers[containerId]);
        }

        public static void Subscribe(int containerId, Action<int, Container> subscriber)
        {
            if (subscriber == null)
                return;

            if (subscribedListeners.ContainsKey(containerId))
            {
                if (!subscribedListeners[containerId].Contains(subscriber))
                    subscribedListeners[containerId].Add(subscriber);
            }
            else
            {
                subscribedListeners[containerId] = new List<Action<int, Container>>();
                subscribedListeners[containerId].Add(subscriber);
            }
        }
            
        public static void UnSubscribe(int containerId, Action<int, Container> unsubscriber)
        {
            if (unsubscriber == null)
                return;

            if (!subscribedListeners.ContainsKey(containerId))
                return;

            if (subscribedListeners[containerId].Contains(unsubscriber) && unsubscriber != null)
                subscribedListeners[containerId].Remove(unsubscriber);
        }

        public static void RemoveAllSubscribers(int containerId)
        {
            subscribedListeners[containerId].Clear();
        }
    }

    public static Dictionary<int, Container> LoadedContainers = new Dictionary<int, Container>();
        
    public static IEnumerator UpdateDatabaseLoadedContainers()
    {
        Dictionary<int, Container> LoadedContainersCopy = new Dictionary<int, Container>(LoadedContainers);

        string sqlCommand = "";
        foreach (KeyValuePair<int, Container> IdContainer in LoadedContainersCopy)
            sqlCommand += $"UPDATE containers SET slots = '{IdContainer.Value.ToJson()}' WHERE id = '{IdContainer.Key}';";

        yield return Api.SqlRequest($"/sql/?request={sqlCommand}").SendWebRequest();
        Debug.Log($"Updated all items");
        yield return null;
    }

    public static IEnumerator SwapSlot(int containerId, int oldSlot, int newSlot)
    {
        yield return MoveSlotData(containerId, oldSlot, newSlot);
    }

    public static void SaveContainer(int containerId, Container container)
    {
        LoadedContainers[containerId] = container;
        ContainerUpdateNotifier.Notify(containerId);
    }

    public static IEnumerator TruncateContainer(int containerId)
    {
        yield return LoadContainer(containerId);
        Container container = LoadedContainers[containerId];

        container.Truncate();

        SaveContainer(containerId, container);
    }

    public static IEnumerator LoadContainer(int containerId)
    {
        if (LoadedContainers.ContainsKey(containerId))
        {
            yield break;
        }

        var www = Api.Request($"/containers/{containerId}");
        yield return www.SendWebRequest();
        var result = Api.GetResult(www);
        
        if (result != null)
        {
            LoadedContainers[containerId] = new Container(result);
        }

        yield return null;
    }

    public static IEnumerator MoveAllItemsInNewContainer(List<int> containersId, int newContainerId)
    {
        List<Container> containers = new List<Container>();

        foreach (int containerId in containersId)
        {
            yield return LoadContainer(containerId);
            containers.Add(LoadedContainers[containerId]);
        }

        List<ContainerSlot> filledSlots = new List<ContainerSlot>();

        foreach (Container container in containers)
            filledSlots.AddRange(container.GetFilledSlots());

        int requiredSlotCount = filledSlots.Count;

        Container newContainer = new Container(requiredSlotCount);

        for (int i = 0; i < requiredSlotCount; i++)
            newContainer[i].CopyContent(filledSlots[i]);

        for (int i = 0; i < containersId.Count; i++)
        {
            containers[i].Truncate();
            SaveContainer(containersId[i], containers[i]);
        }

        SaveContainer(newContainerId, newContainer);
    }

    public static IEnumerator GiveContainerItem(int containerId, int itemId)
    {
        yield return GiveContainerItem(containerId, itemId, 1);
    }

    //Создает и помещает такой то предмет в контейнер
    public static IEnumerator GiveContainerItem(int containerId, int itemId, int count)
    {
        if (itemId <= 0 || count <= 0 || containerId <= 0)
            yield break;

        yield return LoadContainer(containerId);
        Container container = LoadedContainers[containerId];

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
                        SaveContainer(containerId, container);
                        yield break;
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
                            SaveContainer(containerId, container);
                            yield break;
                        }
                    }
                }
            }
            SaveContainer(containerId, container);

            //Все слоты забиты под фулл
            yield return CreateItemInEmptySlot(containerId, itemId, countItems);
        }
        else
            //Такого предмета вообще нет в инвентаре
            yield return CreateItemInEmptySlot(containerId, itemId, count);
    }

    private static IEnumerator CreateItemInEmptySlot(int containerId, int itemId, int count)
    {
        if (itemId <= 0 || count <= 0 || containerId <= 0)
            yield break;

        yield return LoadContainer(containerId);
        Container container = LoadedContainers[containerId];

        Item item = ItemDatabase.GetItemById(itemId);

        int requiredSlotCount = count / item.maxInStack;
        requiredSlotCount += count % item.maxInStack == 0 ? 0 : 1;

        if (container.GetEmptySlotsCount() < requiredSlotCount)
            yield break;

        int itemCounts = count;
        for (int i = 0; i < requiredSlotCount; i++)
        {
            int creatingItemCount = itemCounts > item.maxInStack ? item.maxInStack : itemCounts;
            itemCounts -= creatingItemCount;

            var www = CreateItem(containerId, itemId, creatingItemCount);
            yield return www.SendWebRequest();
            int objectId = Api.GetResult(www)["id"].AsInt;

            yield return MoveObjectToEmptySlot(containerId, objectId);
        }
    }

    public static IEnumerator RemoveItemsFromInventory(int containerId, int itemId, int count)
    {
        yield return LoadContainer(containerId);
        Container container = LoadedContainers[containerId];

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
                        slot.objectId = 0;
                    }

                    SaveContainer(containerId, container);
                    
                    yield break;
                }
                else
                {
                    count -= slot.itemQuantity;

                    slot.itemQuantity = 0;
                    slot.itemId = 0;
                    slot.objectId = 0;
                    
                    continue;
                }
            }
        }

        SaveContainer(containerId, container);
    }

    private static IEnumerator MoveObjectToEmptySlot(int containerId, int objectId)
    {
        yield return LoadContainer(containerId);
        Container container = LoadedContainers[containerId];

        if (container == null)
        {
            yield break;
        }

        ContainerSlot slot = container.GetEmptySlot();

        if (slot == null)
        {
            yield break;
        }

        var www = GetSlotInfo(objectId);
        yield return www.SendWebRequest();
        ContainerSlot info = new ContainerSlot(Api.GetResult(www));
        
        slot.itemId = info.itemId;
        slot.itemQuantity = info.itemQuantity;
        
        SaveContainer(containerId, container);
    }

    private static UnityWebRequest CreateItem(int containerId, int itemId, int count)
    {
        if (itemId <= 0 || count <= 0)
            return null;

        return Api.Request($"/items/?quantity={count}&container_id={containerId}&item_id={itemId}", ApiMethod.POST);
    }

    public static IEnumerator MoveSlotData(int containerId, int from, int to)
    {
        yield return LoadContainer(containerId);
        Container container = LoadedContainers[containerId];

        if (container == null)
            yield break;

        int containerSlotsCount = container.slots.Length;

        if (from < 0 || to < 0 || from > containerSlotsCount || to > containerSlotsCount)
            yield break;

        MoveSlotData(container[from], container[to]);

        SaveContainer(containerId, container);

        yield break;
    }

    public static IEnumerator MoveSlotData(int oldContainerId, int oldSlot, int containerId, int slot)
    {
        yield return LoadContainer(oldContainerId);
        Container oldContainer = LoadedContainers[oldContainerId];

        yield return LoadContainer(containerId);
        Container container = LoadedContainers[containerId];

        if (container == null || oldContainer == null)
            yield break;

        int oldContainerSlotsCount = oldContainer.slots.Length;
        int containerSlotsCount = container.slots.Length;

        if (oldSlot < 0 || slot < 0 || oldSlot > oldContainerSlotsCount || slot > containerSlotsCount)
            yield break;

        MoveSlotData(oldContainer[oldSlot], container[slot]);

        SaveContainer(containerId, container);
        SaveContainer(oldContainerId, oldContainer);
    }

    private static void MoveSlotData(ContainerSlot oldContainerSlot, ContainerSlot containerSlot)
    {
        if (oldContainerSlot.itemId != containerSlot.itemId)
            SwapSlotData(oldContainerSlot, containerSlot);
        else
        {
            Item item = ItemDatabase.GetItemById(containerSlot.itemId);
            if (containerSlot.itemQuantity + oldContainerSlot.itemQuantity <= item.maxInStack)
            {
                containerSlot.itemQuantity += oldContainerSlot.itemQuantity;

                oldContainerSlot.itemId = 0;
                oldContainerSlot.objectId = 0;
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
                    oldContainerSlot.objectId = 0;
                }
            }
        }
    }

    private static void SwapSlotData(ContainerSlot oldContainerSlot, ContainerSlot containerSlot)
    {
        int oldItemId = oldContainerSlot.itemId;
        int oldObjectItemId = oldContainerSlot.objectId;
        int oldQuantity = oldContainerSlot.itemQuantity;

        oldContainerSlot.itemId = containerSlot.itemId;
        oldContainerSlot.objectId = containerSlot.objectId;
        oldContainerSlot.itemQuantity = containerSlot.itemQuantity;

        containerSlot.itemId = oldItemId;
        containerSlot.objectId = oldObjectItemId;
        containerSlot.itemQuantity = oldQuantity;
    }

    private static UnityWebRequest GetSlotInfo(int objectId)
    {
        return Api.Request($"/items/{objectId}");
    }
}
