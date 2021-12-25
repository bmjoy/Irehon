using Sirenix.OdinInspector;
using UnityEngine;

namespace Irehon.Interactable
{
    public class InteractLink : SerializedMonoBehaviour, IInteractable
    {
        public IInteractable interactableOrigin;
        public void Interact(Player player)
        {
            this.interactableOrigin.Interact(player);
        }

        public void StopInterract(Player player)
        {
            this.interactableOrigin.StopInterract(player);
        }
    }
}