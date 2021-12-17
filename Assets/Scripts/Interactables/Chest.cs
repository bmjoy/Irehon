using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Chest : NetworkBehaviour, IInteractable
{
    [SerializeField]
    private Container container;
    public Container Container { get => container; }
    public InteractEventHandler OnDestroyEvent;

    private void Awake()
    {
        gameObject.layer = 12;
    }
    public virtual void SetChestContainer(Container container)
    {
        this.container = container;
        OnContainerSet();
    }

    public virtual void Interact(Player player)
    {
        player.GetComponent<PlayerContainerController>().OpenChest(this, container);
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
        OnDestroyEvent?.Invoke(this);
    }
}
