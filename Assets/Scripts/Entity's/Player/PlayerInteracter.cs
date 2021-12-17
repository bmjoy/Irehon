using Mirror;
using System.Collections;
using System.Collections.Generic;
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
        player = GetComponent<Player>();
        model = player.PlayerBonesLinks.Model;
    }

    private void FixedUpdate()
    {
        if (isServer && currentInteractable != null && Vector3.Distance(interractPosition, model.position) > 10f)
        {
            StopInterracting();
        }
    }

    [ServerCallback]
    public void InterractAttemp(NetworkIdentity interractObjectIdentity)
    {
        if (currentInteractable != null)
            return;

        Vector3 currentPos = model.position;

        if (Vector3.Distance(interractObjectIdentity.transform.position, currentPos) > 10f)
            return;

        currentInteractable = interractObjectIdentity.GetComponent<IInteractable>();

        if (currentInteractable == null)
            return;

        isInteracting = true;
        interractPosition = interractObjectIdentity.transform.position;
        currentInteractable.Interact(player);
    }

    [ServerCallback]
    public void StopInterracting()
    {
       if (currentInteractable != null)
            currentInteractable.StopInterract(player);

        currentInteractable = null;
        isInteracting = false;
    }

    [Command]
    public void StopInterractRpc()
    {
        StopInterracting();
    }
}
