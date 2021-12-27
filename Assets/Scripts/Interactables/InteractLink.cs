using Sirenix.OdinInspector;
using UnityEngine;

namespace Irehon.Interactable
{
    public class InteractLink : Interactable
    {
        public Interactable interactableOrigin;
        public override void Interact(Player player)
        {
            this.interactableOrigin.Interact(player);
        }

        public override void StopInterract(Player player)
        {
            this.interactableOrigin.StopInterract(player);
        }
    }
}