using Irehon;
using UnityEngine;

public class PlayerIdleState : PlayerRotatableState
{
    public PlayerIdleState(Player player) : base(player)
    {
    }

    public override bool CanRotateCamera => true;

    public override float MovementSpeed => 0;

    public override bool CanInteract => true;
    public override PlayerStateType Type => PlayerStateType.Idle;
    public override void Enter(bool isResimulating)
    {
    }

    public override void Exit(bool isResimulating)
    {
    }

    public override PlayerStateType HandleInput(InputInfo input, bool isServer)
    {
        base.HandleInput(input, isServer);

        if (input.IsKeyPressed(KeyCode.Space))
        {
            return PlayerStateType.Jump;
        }

        if (input.IsKeyPressed(KeyCode.V))
        {
            return PlayerStateType.Dance;
        }

        if (input.GetMoveVector() != Vector2.zero)
        {
            if (input.IsKeyPressed(KeyCode.LeftShift) && input.GetMoveVector().x == 0 && input.GetMoveVector().y > 0)
            {
                return PlayerStateType.Run;
            }

            return PlayerStateType.Walk;
        }
        return this.Type;
    }
}
