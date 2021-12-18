namespace Irehon.Interactable
{
    public delegate void InteractEventHandler(IInteractable sender);
    public interface IInteractable
    {
        void Interact(Player player);
        void StopInterract(Player player);
    }
}