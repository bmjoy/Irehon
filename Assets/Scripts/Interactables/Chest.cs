using Mirror;
using UnityEngine;

namespace Irehon.Interactable
{
    public class Chest : NetworkBehaviour, IInteractable
    {
        [SerializeField]
        private Container container;
        public Container Container => this.container;
        public InteractEventHandler OnDestroyEvent;

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
            player.GetComponent<PlayerContainerController>().OpenChest(this, this.container);
        }

        public virtual void StopInterract(Player player)
        {
            player.GetComponent<PlayerContainerController>().CloseChest(this);
        }

        protected virtual void OnContainerSet()
        {

        }

        protected virtual void OnDestroy()
        {
            this.OnDestroyEvent?.Invoke(this);
        }
    }
}