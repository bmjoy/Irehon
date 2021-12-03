using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class OnContainerUpdate : UnityEvent<Container> { }

public class Chest : NetworkBehaviour, IInteractable
{
    [SerializeField]
    private int containerId;
    public int ContainerId { get => containerId; }
    public OnContainerUpdate OnContainerUpdate { get; } = new OnContainerUpdate();
    public UnityEvent OnDestroyEvent { get; } = new UnityEvent();

    private void Awake()
    {
        gameObject.layer = 12;
    }

    private void Start()
    {
        if (containerId != 0)
            ContainerData.ContainerUpdateNotifier.Subscribe(containerId, ContainerUpdateEvent);
    }

    private void ContainerUpdateEvent(int containerId, Container container) => OnContainerUpdate.Invoke(container);

    public virtual void SetChestId(int containerId)
    {
        if (containerId != 0)
            ContainerData.ContainerUpdateNotifier.Subscribe(containerId, ContainerUpdateEvent);

        if (this.containerId != 0)
            ContainerData.ContainerUpdateNotifier.UnSubscribe(containerId, ContainerUpdateEvent);

        this.containerId = containerId;
    }

    public virtual void Interact(Player player)
    {
        player.GetComponent<PlayerContainerController>().OpenChest(this, containerId);
    }

    public virtual void StopInterract(Player player)
    {
        player.GetComponent<PlayerContainerController>().CloseChest();
    }

    protected virtual void OnDestroy()
    {
        if (containerId != 0)
            ContainerData.ContainerUpdateNotifier.UnSubscribe(containerId, ContainerUpdateEvent);

        OnDestroyEvent.Invoke();
    }
}
