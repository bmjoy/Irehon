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

    public class ContainerData : IContainerData
    {
        public static IContainerData i;

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
            if (!AssingItemToContainer(containerId, objectId))
                return false;
            return true;
        }

        public bool GiveCharacterItem(Character character, int itemId)
        {
            if (itemId <= 0)
                return false;
            int containerId = GetCharacterContainer(character.id);
            return GiveContainerItem(containerId, itemId);
        }

        public bool GiveCharacterItem(int characterId, int itemId)
        {
            if (itemId <= 0)
                return false;
            int containerId = GetCharacterContainer(characterId);
            return GiveContainerItem(containerId, itemId);
        }

        //Создает и помещает такой то предмет в контейнер
        public bool GiveContainerItem(int containerId, int itemId)
        {
            if (itemId <= 0)
                return false;
            if (EmptySlotCount(containerId) == 0)
                return false;
            int objectId = CreateItem(containerId, itemId);
            AssingItemToContainer(containerId, objectId);
            return true;
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
            SaveContainer(containerId, container);
            return true;
        }

        private bool AssingItemToContainer(int containerId, int objectId)
        {
            Container container = GetContainer(containerId);
            if (container == null)
                return false;
            var slot = container.GetEmptySlot();
            if (slot == null)
                return false;
            slot.itemId = GetItemIdById(objectId);
            slot.objectId = objectId;
            SaveContainer(containerId, container);
            return true;
        }

        private int CreateItem(int containerId, int itemId)
        {
            if (itemId <= 0)
                return 0;
            Dictionary<string, string> values = new Dictionary<string, string>()
            {
                ["item_id"] = itemId.ToString(),
                ["container_id"] = containerId.ToString()
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
            int oldItemId = container[from].itemId;
            int oldObjectItemId = container[from].objectId;

            container[from].itemId = container[to].itemId;
            container[from].objectId = container[to].objectId;

            container[to].itemId = oldItemId;
            container[to].objectId = oldObjectItemId;
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
            int oldItemId = oldContainer[oldSlot].itemId;
            int oldObjectItemId = oldContainer[oldSlot].objectId;

            container[oldSlot].itemId = container[slot].itemId;
            container[oldSlot].objectId = container[slot].objectId;

            oldContainer[slot].itemId = oldItemId;
            oldContainer[slot].objectId = oldObjectItemId;
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

        private int GetItemIdById(int objectId)
        {
            return Convert.ToInt32(connection.SingleSelect("object_items", "item_id", "id", objectId.ToString()));
        }
    }
}
