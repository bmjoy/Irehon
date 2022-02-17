using Irehon;
using Irehon.Interactable;
using Mirror;
using UnityEngine;

public class PlayerInteracter : NetworkBehaviour
{
    public static GameObject LocalInteractObject;
    public static PlayerInteracter LocalInstance { get; private set; }
    public Interactable currentInteractable { get; private set; }

    [SyncVar, HideInInspector]
    public bool isInteracting;

    private Vector3 interractPosition;

    private Player player;
    private Transform model;

    private void Start()
    {
        this.player = this.GetComponent<Player>();
        this.model = this.player.PlayerBonesLinks.Model;

        if (isLocalPlayer)
            LocalInstance = this;
    }

    private void FixedUpdate()
    {
        if (this.isServer && this.currentInteractable != null && Vector3.Distance(this.interractPosition, this.model.position) > 10f)
        {
            this.StopInterracting();
        }
    }

    [ServerCallback]
    public void InterractAttemp(NetworkIdentity interactObjectIdentity)
    {
        if (this.currentInteractable != null)
        {
            return;
        }

        if (Vector3.Distance(interactObjectIdentity.transform.position, this.model.position) > 10f)
        {
            return;
        }

        this.currentInteractable = interactObjectIdentity.GetComponent<Interactable>();

        if (this.currentInteractable == null)
        {
            return;
        }

        this.isInteracting = true;
        this.interractPosition = interactObjectIdentity.transform.position;
        this.currentInteractable.Interact(this.player);
        TargetSetInteractObject(interactObjectIdentity.gameObject);
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
        TargetSetInteractObject(null);
    }

    [TargetRpc]
    private void TargetSetInteractObject(GameObject interact)
    {
        LocalInteractObject = interact;
    }

    [Command]
    public void StopInterractCommand()
    {
        this.StopInterracting();
    }
}
