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
    public OnContainerUpdate OnContainerUpdate { get; } = new OnContainerUpdate();

    private void Start()
    {
        if (containerId != 0)
            ContainerData.ContainerUpdateNotifier.Subscribe(containerId, ContainerUpdateEvent);
    }

    private void ContainerUpdateEvent(int containerId, Container container) => OnContainerUpdate.Invoke();

    public void SetChestId(int containerId)
    {
        if (containerId != 0)
            ContainerData.ContainerUpdateNotifier.Subscribe(containerId, ContainerUpdateEvent);

        if (this.containerId != 0)
            ContainerData.ContainerUpdateNotifier.UnSubscribe(containerId, ContainerUpdateEvent);

        this.containerId = containerId;
    }

    public void Interact(Player player)
    {
        player.GetComponent<PlayerContainerController>().OpenChest(this);
    }
}
