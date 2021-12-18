using Irehon.Interactable;
using Mirror;
using UnityEngine;

public class PlayerInteracter : NetworkBehaviour
{
    [SyncVar, HideInInspector]
    public bool isInteracting;

    private IInteractable currentInteractable;
    private Vector3 interractPosition;

    private Player player;
    private Transform model;

    private void Start()
    {
        this.player = this.GetComponent<Player>();
        this.model = this.player.PlayerBonesLinks.Model;
    }

    private void FixedUpdate()
    {
        if (this.isServer && this.currentInteractable != null && Vector3.Distance(this.interractPosition, this.model.position) > 10f)
        {
            this.StopInterracting();
        }
    }

    [ServerCallback]
    public void InterractAttemp(NetworkIdentity interractObjectIdentity)
    {
        if (this.currentInteractable != null)
        {
            return;
        }

        if (Vector3.Distance(interractObjectIdentity.transform.position, this.model.position) > 10f)
        {
            return;
        }

        this.currentInteractable = interractObjectIdentity.GetComponent<IInteractable>();

        if (this.currentInteractable == null)
        {
            return;
        }

        this.isInteracting = true;
        this.interractPosition = interractObjectIdentity.transform.position;
        this.currentInteractable.Interact(this.player);
    }

    [ServerCallback]
    public void StopInterracting()
    {
        if (this.currentInteractable != null)
        {
            this.currentInteractable.StopInterract(this.player);
        }

        this.currentInteractable = null;
        this.isInteracting = false;
    }

    [Command]
    public void StopInterractRpc()
    {
        this.StopInterracting();
    }
}
