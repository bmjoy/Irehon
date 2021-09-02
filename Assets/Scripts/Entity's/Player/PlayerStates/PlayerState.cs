using System;
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
    public abstract PlayerStateType Type { get; }

    protected Player player;
    public virtual void Update() { }
    public abstract void Enter();
    public abstract void Exit();
    public abstract PlayerStateType HandleInput(InputInfo input, bool isServer);

    public override bool Equals(object obj)
    {
        return obj is PlayerState state &&
               Type == state.Type;
    }

    public override int GetHashCode()
    {
        int hashCode = -837895853;
        hashCode = hashCode * -1521134295 + CanTakeDamage.GetHashCode();
        hashCode = hashCode * -1521134295 + CanRotateCamera.GetHashCode();
        hashCode = hashCode * -1521134295 + MovementSpeed.GetHashCode();
        hashCode = hashCode * -1521134295 + CanInteract.GetHashCode();
        hashCode = hashCode * -1521134295 + Type.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<Player>.Default.GetHashCode(player);
        return hashCode;
    }
}
