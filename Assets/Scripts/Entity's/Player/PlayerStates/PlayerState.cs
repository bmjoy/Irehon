using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerState
{
    public PlayerState(Player player) 
    {
        this.player = player;
    }

    public bool CanRotateCamera { get; private set; }
    public float MovementSpeed { get; private set; }
    public bool CanInteract { get; private set; }

    protected Player player;
    public virtual void Enter() { }
    public virtual void Exit() { }
    public abstract void ClientHandleInput(InputInfo input);
    public abstract void ServerHandleInput(InputInfo input);
}
