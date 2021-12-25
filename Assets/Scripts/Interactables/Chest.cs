using Irehon.UI;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon.Interactable
{
    public class Chest : NetworkBehaviour, IInteractable
    {
        public delegate void ChestEventHandler();
        private Container container;
        public Container Container => this.container;
        public event ChestEventHandler Destroyed;
        protected List<Player> interacting { get; private set; } = new List<Player>();

        private void Awake()
        {
            this.gameObject.layer = 12;
        }
        public virtual void SetChestContainer(Container container)
        {
            this.container = container;
            this.OnContainerSet();
        }

        public virtual void Interact(Player player)
        {
            TargetOpenChest(player.connectionToClient, this.container);
            player.interactContainer = this.container;
            interacting.Add(player);
        }

        public virtual void StopInterract(Player player)
        {
            TargetCloseChest(player.connectionToClient);
            player.interactContainer = null;
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
            this.Destroyed?.Invoke();
        }



        [TargetRpc]
        protected virtual void TargetCloseChest(NetworkConnection target)
        {
            Player.LocalInteractContainer = null;
            Destroyed -= ChestWindow.Instance.CloseChest;
            ChestWindow.Instance.CloseChest();
        }

        [TargetRpc]
        protected virtual void TargetOpenChest(NetworkConnection target, Container container)
        {
            Destroyed += ChestWindow.Instance.CloseChest;
            Player.LocalInteractContainer = container;
            ChestWindow.Instance.OpenChest(container);
        }
    }
}