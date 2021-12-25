using Irehon.UI;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon.Interactable
{
    public class LootBag : NetworkBehaviour, IInteractable
    {
        public delegate void ChestEventHandler();
        public event ChestEventHandler Destroyed;
        private Container container;
        public Container Container => this.container;
        protected List<Player> interacting { get; private set; } = new List<Player>();

        public void Interact(Player player)
        {
            TargetSendLootBag(player.connectionToClient, this.container);
            interacting.Add(player);
        }

        public void StopInterract(Player player)
        {
            TargetCloseLootBag(player.connectionToClient);
            interacting.Remove(player);
        }

        public virtual void SetChestContainer(Container container)
        {
            this.container = container;
            this.OnContainerSet();
        }

        protected virtual void OnContainerSet()
        {
            container.ContainerSlotsChanged += SendContainerInteractingPlayer;
        }

        protected virtual void SendContainerInteractingPlayer(Container container)
        {
            foreach (Player player in interacting)
            {
                if (player != null)
                    TargetSendLootBag(player.connectionToClient, container);
                else
                    interacting.Remove(player);
            }
        }

        protected virtual void OnContainerUpdate()
        {

        }

        [TargetRpc]
        public void TargetSendLootBag(NetworkConnection target, Container container)
        {
            LootWindow.Instance.OpenLootBag(container);
        }

        [TargetRpc]
        public void TargetCloseLootBag(NetworkConnection target)
        {
            LootWindow.Instance.CloseLootBag();
        }

        protected virtual void OnDestroy()
        {
            this.Destroyed?.Invoke();
        }

        [Command(requiresAuthority = false)]
        public void ClaimItem(int index, int count, NetworkConnectionToClient sender = null)
        {
            print($"{index} claimning {count}");
            Player player = sender.identity.gameObject.GetComponent<Player>();

            if (player.GetComponent<PlayerInteracter>().currentInteractable != this)
                return;

            if (index < 0 || index >= container.slots.Length || container[index].itemId == 0)
            {
                return;
            }
            print($"{index} claimning slot {count}");

            player.inventory.ClaimFromSlot(container, index, count);
        }
    }
}