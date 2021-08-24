using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerState
{
    public PlayerState(Player player) 
    {
        this.player = player;
    }
    public virtual bool CanTakeDamage => true;
    public abstract bool CanRotateCamera { get; }
    public abstract float MovementSpeed { get; }
    public abstract bool CanInteract { get; }

    protected Player player;
    public virtual void Update() { }
    public abstract void Enter();
    public abstract void Exit();
    public abstract PlayerState HandleInput(InputInfo input, bool isServer);
}
