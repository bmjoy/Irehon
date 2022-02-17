using Irehon.UI;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon.Interactable
{
    public class Chest : Interactable
    {
        public delegate void ChestEventHandler();
        private Container container;
        public Container Container => this.container;
        public event ChestEventHandler Destroyed;
        protected List<Player> interacting { get; private set; } = new List<Player>();

        public virtual void SetChestContainer(Container container)
        {
            this.container = container;
            this.OnContainerSet();
        }

        public override void Interact(Player player)
        {
            TargetOpenChest(player.connectionToClient, this.container);
            player.GetComponent<PlayerContainers>().interactContainer = this.container;
            interacting.Add(player);
        }

        public override void StopInterract(Player player)
        {
            TargetCloseChest(player.connectionToClient);
            player.GetComponent<PlayerContainers>().interactContainer = null;
            interacting.Remove(player);
        }

        protected virtual void OnContainerSet()
        {
            container.ContainerSlotsChanged += SendContainerInteractingPlayer;
        }

        protected virtual void OnContainerUpdate()
        {

        }

        protected virtual void SendContainerInteractingPlayer(Container container)
        {
            foreach (Player player in interacting)
            {
                if (player != null)
                    TargetOpenChest(player.connectionToClient, container);
                else
                    interacting.Remove(player);
            }
        }


        protected virtual void OnDestroy()
        {
            if (isClient && PlayerInteracter.LocalInteractObject == gameObject)
                ChestWindow.Instance.CloseChest();

            this.Destroyed?.Invoke();
        }

        [TargetRpc]
        protected virtual void TargetCloseChest(NetworkConnection target)
        {
            PlayerContainers.LocalInteractContainer = null;
            Destroyed -= ChestWindow.Instance.CloseChest;
            ChestWindow.Instance.CloseChest();
        }

        [TargetRpc]
        protected virtual void TargetOpenChest(NetworkConnection target, Container container)
        {
            Destroyed += ChestWindow.Instance.CloseChest;
            PlayerContainers.LocalInteractContainer = container;
            ChestWindow.Instance.OpenChest(container);
        }
    }
}