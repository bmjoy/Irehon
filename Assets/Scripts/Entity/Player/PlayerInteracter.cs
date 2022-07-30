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
    }

    [TargetRpc]
    private void TargetSetInteractObject(GameObject interact)
    {
        LocalInteractObject = interact;
    }

    [Command]
    public void StopInterractCommand()
    {
    }
}
