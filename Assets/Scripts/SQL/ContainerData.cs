using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MySql
{
    public interface IContainerData
    {
        int CreateContainer(int capacity);
        Container GetContainer(int containerId);
        void SwapSlot(int containerId, int oldSlot, int newSlot);
        void MoveSlot(int oldContainerId, int oldSlot, int containerId, int slot);
        void SaveContainer(int containerId, Container container);
        int GetCharacterContainer(int characterId);
        bool GiveContainerItem(int containerId, int itemId);
        bool RemoveItem(int containerId, int objectId);
        bool ChangeItemOwner(int oldContainerId, int containerId, int objectId);
        bool GiveCharacterItem(int characterId, int itemId);
        bool GiveCharacterItem(Character character, int itemId);

    }

    public class ContainerData
    {
        public static ContainerData i;

        private Connection connection;

        public ContainerData(Connection connection)
        {
            if (i == null)
                i = this;
            this.connection = connection;
        }

        public int CreateContainer(int capacity)
        {
            Container container = new Container(capacity);
            return Convert.ToInt32(connection.Insert("containers", "slots", container.ToJson()));
        }

        public void MoveSlot(int oldContainerId, int oldSlot, int containerId, int slot)
        {
            MoveSlotData(oldContainerId, oldSlot, containerId, slot);
        }

        public void SwapSlot(int containerId, int oldSlot, int newSlot)
        {
            MoveSlotData(containerId, oldSlot, newSlot);
        }

        public void SaveContainer(int containerId, Container container)
        {
            Connection.instance.UpdateColumn("containers", "id", containerId.ToString(), "slots", container.ToJson());
        }

        public int GetCharacterContainer(int characterId)
        {
            return Convert.ToInt32(connection.SingleSelect("characters", "container_id", "c_id", characterId.ToString()));
        }

        public int GetItemOwner(int objectId)
        {
            return Convert.ToInt32(connection.SingleSelect("object_items", "container_id", "id", objectId.ToString()));
        }

        public Container GetContainer(int containerId)
        {
            string json = connection.SingleSelect("containers", "slots", "id", containerId.ToString());
            if (json == null)
                return null;
            return new Container(json);
        }

        public bool ChangeItemOwner(int oldContainerId, int containerId, int objectId)
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

        public bool GiveCharacterItem(Character character, int itemId, int count)
        {
            if (itemId <= 0)
                return false;
            int containerId = GetCharacterContainer(character.id);
            return GiveContainerItem(containerId, itemId, count);
        }

        public bool GiveCharacterItem(Character character, int itemId)
        {
            if (itemId <= 0)
                return false;
            int containerId = GetCharacterContainer(character.id);
            return GiveContainerItem(containerId, itemId, 1);
        }

        public bool GiveCharacterItem(int characterId, int itemId, int count)
        {
            if (itemId <= 0)
                return false;
            int containerId = GetCharacterContainer(characterId);
            return GiveContainerItem(containerId, itemId, count);
        }

        public bool GiveCharacterItem(int characterId, int itemId)
        {
            if (itemId <= 0)
                return false;
            int containerId = GetCharacterContainer(characterId);
            return GiveContainerItem(containerId, itemId, 1);
        }

        //Создает и помещает такой то предмет в контейнер
        public bool GiveContainerItem(int containerId, int itemId, int count)
        {
            if (itemId <= 0)
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
        public bool RemoveItem(int containerId, int objectId)
        {
            if (UnassignItemFromContainer(containerId, objectId))
            {
                DeleteItem(objectId);
                return true;
            }
            return false;
        }

        private bool CreateItemInEmptySlot(int containerId, int itemId, int count)
        {
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

        private bool UnassignItemFromContainer(int containerId, int objectId)
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

        private bool MoveObjectToEmptySlot(int containerId, int objectId)
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

        private int CreateItem(int containerId, int itemId, int count)
        {
            if (itemId <= 0)
                return 0;
            Dictionary<string, string> values = new Dictionary<string, string>()
            {
                ["item_id"] = itemId.ToString(),
                ["container_id"] = containerId.ToString(),
                ["quantity"] = count.ToString()
            };
            return Convert.ToInt32(connection.Insert("object_items", values));
        }

        private bool MoveSlotData(int containerId, int from, int to)
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

        private bool MoveSlotData(int oldContainerId, int oldSlot, int containerId, int slot)
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

        private int EmptySlotCount(int containerId)
        {
            Container container = GetContainer(containerId);
            if (container == null)
                return 0;
            return container.GetEmptySlotsCount();
        }

        private void ChangeContainerOnItem(int containerId, int objectId)
        {
            connection.UpdateColumn("object_items", "id", objectId.ToString(), "container_id", containerId.ToString());
        }

        private void DeleteItem(int objectId)
        {
            connection.Delete("object_items", "id", objectId.ToString());
        }

        private void MoveSlotData(ContainerSlot oldContainerSlot, ContainerSlot containerSlot)
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

        private void SwapSlotData(ContainerSlot oldContainerSlot, ContainerSlot containerSlot)
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

        private int GetItemIdById(int objectId)
        {
            return Convert.ToInt32(connection.SingleSelect("object_items", "item_id", "id", objectId.ToString()));
        }

        private int GetItemCount(int objectId)
        {
            return Convert.ToInt32(connection.SingleSelect("object_items", "quantity", "id", objectId.ToString()));
        }

        private ContainerSlot GetSlotInfo(int objectId)
        {
            ContainerSlot containerSlot = new ContainerSlot();

            string[] columns = { "item_id", "quantity" };

            List<string> result = connection.MultipleSelect("object_items", new List<string>(columns), "id", objectId.ToString());

            containerSlot.itemId = Convert.ToInt32(result[0]);
            containerSlot.itemQuantity = Convert.ToInt32(result[1]);
            containerSlot.objectId = objectId;

            return containerSlot;
        }
    }
}
