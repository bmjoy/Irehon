using Irehon;
using Irehon.UI;

public class PlayerDeathState : PlayerState
{
    public PlayerDeathState(Player player) : base(player)
    {
    }

    public override bool CanTakeDamage => false;
    public override bool CanRotateCamera => false;
    public override PlayerStateType Type => PlayerStateType.Death;

    public override float MovementSpeed => 0;

    public override bool CanInteract => false;

    public override void Enter(bool isResimulating)
    {
        if (isResimulating)
        {
            return;
        }

        this.abilitySystem.AbilityInterrupt();
        Canvases.Instance.HideStatusCanvas();
    }

    public override void Exit(bool isResimulating)
    {
        if (isResimulating)
        {
            return;
        }

        if (this.player.isServer)
        {
            this.player.ShowModel();
        }

        Canvases.Instance.ShowStatusCanvas();
    }

    public override PlayerStateType HandleInput(InputInfo input, bool isServer)
    {
        return this.Type;
    }
}
