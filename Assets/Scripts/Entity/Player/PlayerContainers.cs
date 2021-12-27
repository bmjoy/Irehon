using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon
{
    public class PlayerContainers : NetworkBehaviour
    {
        public delegate void PlayerContainerEventHandler(Container container);

        public static event PlayerContainerEventHandler LocalInventorUpdated;
        public static event PlayerContainerEventHandler LocalEquipmentUpdated;

        public static PlayerContainers LocalInstance { get; private set; }
        public static Container LocalInventory { get; private set; }
        public static Container LocalEquipment { get; private set; }
        public static Container LocalInteractContainer;

        public event PlayerContainerEventHandler ShareEquipmentUpdated;

        [SyncVar]
        public Container equipment;
        public Container inventory;
        public Container interactContainer;
        private Player player;

        private async void Start()
        {
            player = GetComponent<Player>();
            if (isClient)
            {
                if (equipment != null)
                    ShareEquipmentUpdated.Invoke(equipment);
            }

            if (isLocalPlayer)
                LocalInstance = this;
        }

        [TargetRpc]
        private void SendInventoryTargetRPC(Container inventory)
        {
            this.inventory = inventory;
            LocalInventory = this.inventory;
            LocalInventorUpdated?.Invoke(inventory);
        }

        [ClientRpc]
        private void SendEquipmentClientRPC(Container equipment)
        {
            this.equipment = equipment;
            ShareEquipmentUpdated?.Invoke(equipment);
            if (isLocalPlayer)
            {
                LocalEquipment = this.equipment;
                LocalEquipmentUpdated?.Invoke(equipment);
            }
        }

        public Container GetContainer(ContainerType type)
        {
            if (type == ContainerType.Inventory)
                return inventory;
            if (type == ContainerType.Equipment)
                return equipment;
            if (type == ContainerType.Interact)
                return interactContainer;
            return null;
        }

        //from to
        [Command]
        public void MoveItem(ContainerType firstType, int firstSlot, ContainerType secondType, int secondSlot)
        {
        }
    }
}