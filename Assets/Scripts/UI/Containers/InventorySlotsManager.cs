using System;
using System.Collections;
using UnityEngine;

namespace Irehon.UI
{
    public class InventorySlotsManager : MonoBehaviour
    {
        public static InventorySlotsManager Instance;
        
        private Player player;

        private void Awake()
        {
            Instance = this;
            Player.LocalPlayerIntialized += player => this.player = player;
        }

        private Container GetLocalContainer(ContainerType type)
        {
            switch (type)
            {
                case ContainerType.Inventory:
                    return PlayerContainers.LocalInventory;
                case ContainerType.Equipment:
                    return PlayerContainers.LocalEquipment;
                case ContainerType.Interact:
                    return PlayerContainers.LocalInteractContainer;
                default:
                    throw new ArgumentException("Invalid container type exception");
            }
        }

        public void MoveSlots(InventorySlotUI from, InventorySlotUI to)
        {
            PlayerContainers.LocalInstance.MoveItem(from.type, from.slotId, to.type, to.slotId);
        }

        public void FastMoveSlot(InventorySlotUI from)
        {
            if (from.type == ContainerType.Interact)
            {
                ContainerSlot slot = PlayerContainers.LocalInventory.GetEmptySlot();
                if (slot != null)
                {
                    PlayerContainers.LocalInstance.MoveItem(from.type, from.slotId, ContainerType.Inventory, slot.slotIndex);
                }
            }

            if (from.type == ContainerType.Inventory)
            {
                if (PlayerContainers.LocalInteractContainer != null)
                {
                    ContainerSlot slot = PlayerContainers.LocalInteractContainer.GetEmptySlot();
                    if (slot != null)
                    {
                        PlayerContainers.LocalInstance.MoveItem(from.type, from.slotId, ContainerType.Interact, slot.slotIndex);
                    }
                }
            }
        }

        public void UseItemSlot(InventorySlotUI slot)
        {
            Item item = slot.item;

            if (item.type == ItemType.Weapon || item.type == ItemType.Armor)
            {
                if (slot.type != ContainerType.Equipment)
                    PlayerContainers.LocalInstance.MoveItem(slot.type, slot.slotId, ContainerType.Equipment, (int)item.equipmentSlot);
                else
                {
                    var emptySlot = PlayerContainers.LocalInventory.GetEmptySlot();
                    if (emptySlot != null)
                        PlayerContainers.LocalInstance.MoveItem(slot.type, slot.slotId, ContainerType.Inventory, emptySlot.slotIndex);
                }
            }
        }
    }
}