using Mirror;

namespace Irehon.Interactable
{
    public delegate void InteractEventHandler(Interactable sender);
    public abstract class Interactable : NetworkBehaviour
    {
        private void Awake()
        {
            this.gameObject.layer = 12;
        }

        public abstract void Interact(Player player);
        public abstract void StopInterract(Player player);
    }
}