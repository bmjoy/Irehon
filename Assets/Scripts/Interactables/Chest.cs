using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class OnContainerUpdate : UnityEvent { }

public class Chest : NetworkBehaviour, IInteractable
{
    [SerializeField]
    private int containerId;
    public int ContainerId { get => containerId; }

    public void SetChestId(int containerId)
    {
        print($"setted chest id {containerId}");
        this.containerId = containerId;
    }

    public OnContainerUpdate OnContainerUpdate { get; } = new OnContainerUpdate();

    public void ContainerUpdateEvent() => OnContainerUpdate.Invoke();

    public void Interact(Player player)
    {
        player.GetComponent<PlayerContainerController>().OpenChest(this);
    }
}
