using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerRotatableState
{
    public PlayerIdleState(Player player) : base(player)
    {
    }

    public override bool CanRotateCamera =>  true;

    public override float MovementSpeed => 0;

    public override bool CanInteract => true;
    public override PlayerStateType Type => PlayerStateType.Idle;
    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    public override PlayerStateType HandleInput(InputInfo input, bool isServer)
    {
        base.HandleInput(input, isServer);

        if (isServer)
            abilitySystem.SendAbilityKeyStatus(input.IsKeyPressed(abilitySystem.ListeningKey), input.TargetPoint);

        if (input.IsKeyPressed(KeyCode.Space))
            return PlayerStateType.Jump;

        if (isServer && input.IsKeyPressed(KeyCode.E))
            playerInteracter.InterractAttemp(input.TargetPoint);

        if (input.GetMoveVector() != Vector2.zero)
        {
            if (input.IsKeyPressed(KeyCode.LeftShift) && input.GetMoveVector().x == 0 && input.GetMoveVector().y > 0)
                return PlayerStateType.Run;

            return PlayerStateType.Walk;
        }
        return this.Type;
    }
}
