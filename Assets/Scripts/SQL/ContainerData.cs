using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MySql
{

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

        public static int CreateContainer(int capacity)
        {
            Container container = new Container(capacity);
            return Convert.ToInt32(Connection.Insert("containers", "slots", container.ToJson()));
        }

        public static int CreateContainer(Container container)
        {
            return Convert.ToInt32(Connection.Insert("containers", "slots", container.ToJson()));
        }

        public static void MoveSlot(int oldContainerId, int oldSlot, int containerId, int slot)
        {
            MoveSlotData(oldContainerId, oldSlot, containerId, slot);
        }

        public static void SwapSlot(int containerId, int oldSlot, int newSlot)
        {
            MoveSlotData(containerId, oldSlot, newSlot);
        }

        public static void SaveContainer(int containerId, Container container)
        {
            Connection.UpdateColumn("containers", "id", containerId.ToString(), "slots", container.ToJson());
        }

        public static int GetCharacterContainer(int characterId)
        {
            string command = "SELECT containers.slots FROM characters " +
                "INNER JOIN containers " +
                $"ON characters.c_id = {characterId} AND containers.id = characters.container_id";
            return Convert.ToInt32(Connection.RecieveSingleData(command));
        }

        public static int GetEquipmentContainer(int characterId)
        {
            return Convert.ToInt32(Connection.SingleSelect("characters", "equipment_id", "c_id", characterId.ToString()));
        }

        public static int GetItemOwner(int objectId)
        {
            return Convert.ToInt32(Connection.SingleSelect("object_items", "container_id", "id", objectId.ToString()));
        }

        public static Container GetContainer(int containerId)
        {
            string json = Connection.SingleSelect("containers", "slots", "id", containerId.ToString());
            if (json == null)
                return null;
            return new Container(json);
        }

        public static bool ChangeItemOwner(int oldContainerId, int containerId, int objectId)
        {
            if (EmptySlotCount(containerId) == 0)
                return false;
            if (!UnassignItemFromContainer(oldContainerId, objectId))
                return false;
            ChangeContainerOnItem(objectId, containerId);
            if (!MoveObjectToEmptySlot(containerId, objectId))
                return false;
            return true;
        }

        public static bool GiveCharacterItem(Character character, int itemId, int count)
        {
            int containerId = GetCharacterContainer(character.id);
            return GiveContainerItem(containerId, itemId, count);
        }

        public static bool GiveCharacterItem(Character character, int itemId)
        {
            int containerId = GetCharacterContainer(character.id);
            return GiveContainerItem(containerId, itemId, 1);
        }

        public static bool GiveCharacterItem(int characterId, int itemId, int count)
        {
            if (characterId <= 0)
                return false;
            int containerId = GetCharacterContainer(characterId);
            return GiveContainerItem(containerId, itemId, count);
        }

        public static bool GiveCharacterItem(int characterId, int itemId)
        {
            if (characterId <= 0)
                return false;
            int containerId = GetCharacterContainer(characterId);
            return GiveContainerItem(containerId, itemId, 1);
        }

        public static void GiveCharacterItem(int characterId, Dictionary<int, int> itemCount)
        {
            if (characterId <= 0)
                return;
            int containerId = GetCharacterContainer(characterId);

            List<Task> tasks = new List<Task>();

            foreach (KeyValuePair<int, int> items in itemCount)
                tasks.Add(Task.Factory.StartNew(() => GiveContainerItem(containerId, items.Key, items.Value)));

            foreach (Task task in tasks)
                task.Wait();
        }

        public static int MoveAllItemsInNewContainer(List<int> containersId)
        {
            List<Container> containers = new List<Container>();
            
            foreach (int containerId in containersId)
                containers.Add(GetContainer(containerId));

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

            return CreateContainer(newContainer);
        }

        //Создает и помещает такой то предмет в контейнер
        public static bool GiveContainerItem(int containerId, int itemId, int count)
        {
            if (itemId <= 0 || count <= 0 || containerId <= 0)
                return false;

            Container container = GetContainer(containerId);
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
                            return true;
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
                                return true;
                            }
                        }
                    }
                }
                SaveContainer(containerId, container);

                //Все слоты забиты под фулл
                return CreateItemInEmptySlot(containerId, itemId, countItems);
            }
            else
                //Такого предмета вообще нет в инвентаре
                return CreateItemInEmptySlot(containerId, itemId, count);
        }

        //Полностью удаляет предмет из контейнера
        public static bool RemoveItem(int containerId, int objectId)
        {
            if (UnassignItemFromContainer(containerId, objectId))
            {
                DeleteItem(objectId);
                return true;
            }
            return false;
        }

        private static bool CreateItemInEmptySlot(int containerId, int itemId, int count)
        {
            if (itemId <= 0 || count <= 0 || containerId <= 0)
                return false;

            Container container = GetContainer(containerId);

            Item item = ItemDatabase.GetItemById(itemId);

            int requiredSlotCount = count / item.maxInStack;
            requiredSlotCount += count % item.maxInStack == 0 ? 0 : 1;

            if (container.GetEmptySlotsCount() < requiredSlotCount)
                return false;

            int itemCounts = count;
            for (int i = 0; i < requiredSlotCount; i++)
            {
                int creatingItemCount = itemCounts > item.maxInStack ? item.maxInStack : itemCounts;
                itemCounts -= creatingItemCount;
                int objectId = CreateItem(containerId, itemId, creatingItemCount);

                MoveObjectToEmptySlot(containerId, objectId);
            }
            return true;
        }

        private static bool UnassignItemFromContainer(int containerId, int objectId)
        {
            Container container = GetContainer(containerId);

            if (container == null)
                return false;

            var slot = container.FindObject(objectId);

            if (slot == null)
                return false;

            slot.objectId = 0;
            slot.itemId = 0;
            slot.itemQuantity = 0;
            SaveContainer(containerId, container);

            return true;
        }

        private static bool MoveObjectToEmptySlot(int containerId, int objectId)
        {
            Container container = GetContainer(containerId);

            if (container == null)
                return false;

            var slot = container.GetEmptySlot();

            if (slot == null)
                return false;

            ContainerSlot info = GetSlotInfo(objectId);
            slot.itemId = info.itemId;
            slot.itemQuantity = info.itemQuantity;
            slot.objectId = objectId;
            SaveContainer(containerId, container);

            return true;
        }

        private static int CreateItem(int containerId, int itemId, int count)
        {
            if (itemId <= 0 || count <= 0)
                return 0;
            Dictionary<string, string> values = new Dictionary<string, string>()
            {
                ["item_id"] = itemId.ToString(),
                ["container_id"] = containerId.ToString(),
                ["quantity"] = count.ToString()
            };
            return Convert.ToInt32(Connection.Insert("object_items", values));
        }

        private static bool MoveSlotData(int containerId, int from, int to)
        {
            Container container = GetContainer(containerId);

            if (container == null)
                return false;

            int containerSlotsCount = container.slots.Length;

            if (from < 0 || to < 0 || from > containerSlotsCount || to > containerSlotsCount)
                return false;

            MoveSlotData(container[from], container[to]);

            SaveContainer(containerId, container);

            return true;
        }

        private static bool MoveSlotData(int oldContainerId, int oldSlot, int containerId, int slot)
        {
            Container oldContainer = GetContainer(oldContainerId);
            Container container = GetContainer(containerId);

            if (container == null || oldContainer == null)
                return false;

            int oldContainerSlotsCount = oldContainer.slots.Length;
            int containerSlotsCount = container.slots.Length;

            if (oldSlot < 0 || slot < 0 || oldSlot > oldContainerSlotsCount || slot > containerSlotsCount)
                return false;

            MoveSlotData(oldContainer[oldSlot], container[slot]);

            SaveContainer(containerId, container);
            SaveContainer(oldContainerId, oldContainer);

            return true;
        }

        private static int EmptySlotCount(int containerId)
        {
            Container container = GetContainer(containerId);
            if (container == null)
                return 0;
            return container.GetEmptySlotsCount();
        }

        private static void ChangeContainerOnItem(int containerId, int objectId)
        {
            Connection.UpdateColumn("object_items", "id", objectId.ToString(), "container_id", containerId.ToString());
        }

        private static void DeleteItem(int objectId)
        {
            Connection.Delete("object_items", "id", objectId.ToString());
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

        private static int GetItemIdById(int objectId)
        {
            return Convert.ToInt32(Connection.SingleSelect("object_items", "item_id", "id", objectId.ToString()));
        }

        private static int GetItemCount(int objectId)
        {
            return Convert.ToInt32(Connection.SingleSelect("object_items", "quantity", "id", objectId.ToString()));
        }

        private static ContainerSlot GetSlotInfo(int objectId)
        {
            ContainerSlot containerSlot = new ContainerSlot();

            string[] columns = { "item_id", "quantity" };

            List<string> result = Connection.MultipleSelect("object_items", new List<string>(columns), "id", objectId.ToString());

            containerSlot.itemId = Convert.ToInt32(result[0]);
            containerSlot.itemQuantity = Convert.ToInt32(result[1]);
            containerSlot.objectId = objectId;

            return containerSlot;
        }
    }
}
