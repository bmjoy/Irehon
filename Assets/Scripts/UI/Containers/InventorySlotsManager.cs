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
                case ContainerType.Chest:
                    return Player.LocalInteractContainer;
                default:
                    throw new ArgumentException("Invalid container type exception");
            }
        }

        public void MoveSlots(InventorySlotUI from, InventorySlotUI to)
        {
            this.playerContainerController.MoveItem(from.type, from.slotId, to.type, to.slotId);
        }

        public void FastMoveSlot(InventorySlotUI from)
        {
            Container firstContainer = GetLocalContainer(from.type);
            if (from.type == ContainerType.Chest)
            {
                ContainerSlot slot = this.playerContainerController.Containers[ContainerType.Inventory].GetEmptySlot();
                if (slot != null)
                {
                    this.playerContainerController.MoveItem(from.type, from.slotId, ContainerType.Inventory, slot.slotIndex);
                }
            }

            if (from.type == ContainerType.Inventory)
            {
                if (this.playerContainerController.Containers.ContainsKey(ContainerType.Chest) && this.playerContainerController.Containers[ContainerType.Chest] != null)
                {
                    ContainerSlot slot = this.playerContainerController.Containers[ContainerType.Chest].GetEmptySlot();
                    if (slot != null)
                    {
                        this.playerContainerController.MoveItem(from.type, from.slotId, ContainerType.Chest, slot.slotIndex);
                    }
                }
            }
        }
    }
}