using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        private static Dictionary<int, List<Action<int, Container>>> subscribedListeners = new Dictionary<int, List<Action<int, Container>>>();

        public static void Notify(int containerId)
        {
            if (!subscribedListeners.ContainsKey(containerId))
                return;
            List<Action<int, Container>> subscribers = subscribedListeners[containerId];

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
            if (subscribedListeners.ContainsKey(containerId))
                subscribedListeners[containerId].Clear();
        }
    }

    public static Dictionary<int, Container> LoadedContainers = new Dictionary<int, Container>();

    public static async void UpdateDatabaseLoadedContainers()
    {
        Dictionary<int, Container> LoadedContainersCopy = new Dictionary<int, Container>(LoadedContainers);

        string sqlCommand = "";
        foreach (KeyValuePair<int, Container> IdContainer in LoadedContainersCopy)
            sqlCommand += $"UPDATE containers SET slots = '{IdContainer.Value.ToJson()}' WHERE id = '{IdContainer.Key}';";

        await Api.SqlRequest($"/sql/?request={sqlCommand}").SendWebRequest();
    }

    public static async Task UpdateLoadedContainer(int id)
    {
        if (!LoadedContainers.ContainsKey(id))
            return;
        string sqlCommand = $"UPDATE containers SET slots = '{LoadedContainers[id].ToJson()}' WHERE id = '{id}';";

        await Api.SqlRequest($"/sql/?request={sqlCommand}").SendWebRequest();
    }

    public static void SwapSlot(int containerId, int oldSlot, int newSlot)
    {
        MoveSlotData(containerId, oldSlot, newSlot);
    }

    public static void SaveContainer(int containerId, Container container)
    {
        LoadedContainers[containerId] = container;
        ContainerUpdateNotifier.Notify(containerId);
    }

    public static void TruncateContainer(int containerId)
    {
        Container container = LoadedContainers[containerId];

        container.Truncate();

        SaveContainer(containerId, container);
    }

    public static async Task LoadContainer(int containerId)
    {
        if (LoadedContainers.ContainsKey(containerId))
            return;

        var www = Api.Request($"/containers/{containerId}");
        await www.SendWebRequest();
        var result = Api.GetResult(www);

        if (result != null)
        {
            LoadedContainers[containerId] = new Container(result);
        }
    }

    public static async Task LoadContainerAsync(int containerId)
    {
        if (LoadedContainers.ContainsKey(containerId))
        {
            return;
        }

        var www = Api.Request($"/containers/{containerId}");
        await www.SendWebRequest();
        var result = Api.GetResult(www);

        if (result != null)
            LoadedContainers[containerId] = new Container(result);
    }

    public static void UnLoadContainer(int containerId)
    {
        if (!LoadedContainers.ContainsKey(containerId))
        {
            return;
        }
        LoadedContainers.Remove(containerId);
    }

    public static void MoveAllItemsInNewContainer(List<int> containersId, int newContainerId)
    {
        List<Container> containers = new List<Container>();

        foreach (int containerId in containersId)
        {
            containers.Add(LoadedContainers[containerId]);
        }

        List<ContainerSlot> filledSlots = new List<ContainerSlot>();

        foreach (Container container in containers)
        {
            filledSlots.AddRange(container.GetFilledSlots());
        }

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

    public static void GiveContainerItem(int containerId, int itemId)
    {
        GiveContainerItem(containerId, itemId, 1);
    }

    //Создает и помещает такой то предмет в контейнер
    public static void GiveContainerItem(int containerId, int itemId, int count)
    {
        if (itemId <= 0 || count <= 0 || containerId <= 0)
            return;

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
                            SaveContainer(containerId, container);
                            return;
                        }
                    }
                }
            }
            SaveContainer(containerId, container);

            //Все слоты забиты под фулл
            CreateItemInEmptySlot(containerId, itemId, countItems);
        }
        else
            //Такого предмета вообще нет в инвентаре
            CreateItemInEmptySlot(containerId, itemId, count);
    }

    private static void CreateItemInEmptySlot(int containerId, int itemId, int count)
    {
        if (itemId <= 0 || count <= 0 || containerId <= 0)
            return;

        Container container = LoadedContainers[containerId];

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

            MoveObjectToEmptySlot(containerId, itemId, creatingItemCount);
        }
    }

    public static void RemoveItemsFromInventory(int containerId, int itemId, int count)
    {
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
                    }

                    SaveContainer(containerId, container);

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

        SaveContainer(containerId, container);
    }

    private static void MoveObjectToEmptySlot(int containerId, int itemId, int itemQuantity)
    {
        Container container = LoadedContainers[containerId];

        if (container == null)
        {
            return;
        }

        ContainerSlot slot = container.GetEmptySlot();

        if (slot == null)
        {
            return;
        }

        slot.itemId = itemId;
        slot.itemQuantity = itemQuantity;

        SaveContainer(containerId, container);
    }

    public static void MoveSlotData(int containerId, int from, int to)
    {
        Container container = LoadedContainers[containerId];

        if (container == null)
            return;

        int containerSlotsCount = container.slots.Length;

        if (from < 0 || to < 0 || from > containerSlotsCount || to > containerSlotsCount)
            return;

        MoveSlotData(container[from], container[to]);

        SaveContainer(containerId, container);

        return;
    }

    public static void MoveSlotData(int oldContainerId, int oldSlot, int containerId, int slot)
    {
        Container oldContainer = LoadedContainers[oldContainerId];

        Container container = LoadedContainers[containerId];

        if (container == null || oldContainer == null)
            return;

        int oldContainerSlotsCount = oldContainer.slots.Length;
        int containerSlotsCount = container.slots.Length;

        if (oldSlot < 0 || slot < 0 || oldSlot > oldContainerSlotsCount || slot > containerSlotsCount)
            return;

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
        if (containerSlot.itemId == 0)
        {
            containerSlot.itemId = oldContainerSlot.itemId;
            Item item = ItemDatabase.GetItemById(containerSlot.itemId);
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
