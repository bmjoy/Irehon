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
                    return Player.LocalInventory;
                case ContainerType.Equipment:
                    return Player.LocalEquipment;
                case ContainerType.Interact:
                    return Player.LocalInteractContainer;
                default:
                    throw new ArgumentException("Invalid container type exception");
            }
        }

        public void MoveSlots(InventorySlotUI from, InventorySlotUI to)
        {
            Player.LocalPlayer.MoveItem(from.type, from.slotId, to.type, to.slotId);
        }

        public void FastMoveSlot(InventorySlotUI from)
        {
            if (from.type == ContainerType.Interact)
            {
                ContainerSlot slot = Player.LocalInventory.GetEmptySlot();
                if (slot != null)
                {
                    Player.LocalPlayer.MoveItem(from.type, from.slotId, ContainerType.Inventory, slot.slotIndex);
                }
            }

            if (from.type == ContainerType.Inventory)
            {
                if (Player.LocalInteractContainer != null)
                {
                    ContainerSlot slot = Player.LocalInteractContainer.GetEmptySlot();
                    if (slot != null)
                    {
                        Player.LocalPlayer.MoveItem(from.type, from.slotId, ContainerType.Interact, slot.slotIndex);
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
                    Player.LocalPlayer.MoveItem(slot.type, slot.slotId, ContainerType.Equipment, (int)item.equipmentSlot);
                else
                {
                    var emptySlot = Player.LocalInventory.GetEmptySlot();
                    if (emptySlot != null)
                        Player.LocalPlayer.MoveItem(slot.type, slot.slotId, ContainerType.Inventory, emptySlot.slotIndex);
                }
            }
        }
    }
}