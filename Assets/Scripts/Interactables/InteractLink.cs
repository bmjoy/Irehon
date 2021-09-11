using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractLink : SerializedMonoBehaviour, IInteractable
{
    [SerializeField]
    private IInteractable interactableOrigin;
    public void Interact(Player player)
    {
        interactableOrigin.Interact(player);
    }

    public void StopInterract(Player player)
    {
        interactableOrigin.StopInterract(player);
    }
}
