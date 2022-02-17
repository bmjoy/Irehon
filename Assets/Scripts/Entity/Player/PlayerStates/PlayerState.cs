using Irehon;
using System.Collections.Generic;

public abstract class PlayerState
{
    public PlayerState(Player player)
    {
        this.player = player;
        this.abilitySystem = player.GetComponent<AbilitySystem>();
        this.playerInteracter = player.GetComponent<PlayerInteracter>();
    }
    public virtual bool CanTakeDamage => true;
    public abstract bool CanRotateCamera { get; }
    public abstract float MovementSpeed { get; }
    public abstract bool CanInteract { get; }
    public bool IsReSimulate;
    public abstract PlayerStateType Type { get; }

    protected Player player;
    protected PlayerInteracter playerInteracter;
    protected AbilitySystem abilitySystem;
    public virtual void Update() { }
    public abstract void Enter(bool isResimulating);
    public abstract void Exit(bool isResimulating);
    public abstract PlayerStateType HandleInput(InputInfo input, bool isServer);

    public override bool Equals(object obj)
    {
        return obj is PlayerState state &&
               this.Type == state.Type;
    }

    public override int GetHashCode()
    {
        int hashCode = -837895853;
        hashCode = hashCode * -1521134295 + this.CanTakeDamage.GetHashCode();
        hashCode = hashCode * -1521134295 + this.CanRotateCamera.GetHashCode();
        hashCode = hashCode * -1521134295 + this.MovementSpeed.GetHashCode();
        hashCode = hashCode * -1521134295 + this.CanInteract.GetHashCode();
        hashCode = hashCode * -1521134295 + this.Type.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<Player>.Default.GetHashCode(this.player);
        return hashCode;
    }
}
