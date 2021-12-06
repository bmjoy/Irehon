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
        if (isServer && currentInteractable != null && Vector3.Distance(interractPosition, model.position) > 7f)
        {
            StopInterracting();
        }
    }

    [ServerCallback]
    public void InterractAttemp(Vector3 interractPos)
    {
        if (currentInteractable != null)
            return;

        Vector3 currentPos = model.position;

        if (Vector3.Distance(interractPos, currentPos) > 10f)
            return;

        RaycastHit hit;

        Vector3 direction = interractPos - currentPos;

        if (!Physics.Raycast(currentPos, direction, out hit, 10f, 1 << 12))
            return;

        currentInteractable = hit.collider.GetComponent<IInteractable>();
        isInteracting = true;
        interractPosition = hit.collider.transform.position;
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
