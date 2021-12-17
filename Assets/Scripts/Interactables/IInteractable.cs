using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void InteractEventHandler(IInteractable sender);

public interface IInteractable 
{
    void Interact(Player player);
    void StopInterract(Player player);
}
 