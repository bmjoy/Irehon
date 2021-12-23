using Irehon.UI;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon.Interactable
{
    public class Chest : NetworkBehaviour, IInteractable
    {
        [SerializeField]
        private Container container;
        public Container Container => this.container;
        public InteractEventHandler Destroyed;
        private List<Player> interacting = new List<Player>();

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

        protected void SendContainerInteractingPlayer(Container container)
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
            this.Destroyed?.Invoke(this);

            print("dedstroy");
            if (isClient)
            {
                print("is client");
                if (PlayerInteracter.LocalInteractObject != null && PlayerInteracter.LocalInteractObject == this.gameObject)
                    ChestWindow.Instance.CloseChest();
            }
        }



        [TargetRpc]
        protected virtual void TargetCloseChest(NetworkConnection target)
        {
            Player.LocalInteractContainer = null;
            ChestWindow.Instance.CloseChest();
        }

        [TargetRpc]
        protected virtual void TargetOpenChest(NetworkConnection target, Container container)
        {
            Player.LocalInteractContainer = container;
            ChestWindow.Instance.OpenChest(container);
        }
    }
}