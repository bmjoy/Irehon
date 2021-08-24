using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerInteractableState
{
    public PlayerIdleState(Player player) : base(player)
    {
    }

    public override bool CanRotateCamera =>  true;

    public override float MovementSpeed => 0;

    public override bool CanInteract => true;

    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    public override PlayerState HandleInput(InputInfo input, bool isServer)
    {
        base.HandleInput(input, isServer);

        if (input.IsKeyPressed(KeyCode.Space))
            return new PlayerJumpingState(player);

        if (isServer && input.IsKeyPressed(KeyCode.E))
            player.InterractAttempToServer(input.TargetPoint);

        if (input.GetMoveVector() != Vector2.zero)
        {
            if (input.IsKeyPressed(KeyCode.LeftShift) && input.GetMoveVector().x == 0 && input.GetMoveVector().y > 0)
                return new PlayerRunState(player);

            return new PlayerWalkState(player);
        }
        return this;
    }
}
