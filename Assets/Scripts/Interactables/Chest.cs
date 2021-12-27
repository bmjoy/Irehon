using Irehon.UI;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon.Interactable
{
    public class Chest : Interactable
    {
        public virtual void SetChestContainer(Container container)
        {
        }

        public override void Interact(Player player)
        {
        }

        public override void StopInterract(Player player)
        {
        }

        protected virtual void OnContainerSet()
        {
        }

        protected virtual void OnContainerUpdate()
        {

        }

        protected virtual void SendContainerInteractingPlayer(Container container)
        {
        }


        protected virtual void OnDestroy()
        {
            if (isClient && PlayerInteracter.LocalInteractObject == gameObject)
                ChestWindow.Instance.CloseChest();
        }

        [TargetRpc]
        protected virtual void TargetCloseChest(NetworkConnection target)
        {
            PlayerContainers.LocalInteractContainer = null;
            ChestWindow.Instance.CloseChest();
        }

        [TargetRpc]
        protected virtual void TargetOpenChest(NetworkConnection target, Container container)
        {
            PlayerContainers.LocalInteractContainer = container;
            ChestWindow.Instance.OpenChest(container);
        }
    }
}