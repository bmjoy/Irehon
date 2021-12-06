using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerDanceState : PlayerRotatableState
{
    public PlayerDanceState(Player player) : base(player)
    {
    }

    public override bool CanRotateCamera => true;

    public override float MovementSpeed => 0;

    public override bool CanInteract => true;
    public override PlayerStateType Type => PlayerStateType.Dance;
    public override void Enter(bool isResimulating)
    {
        if (!isResimulating)
        {
            abilitySystem.animator.SetBool("isDancing", true);
            abilitySystem.animator.applyRootMotion = true;
        }
    }

    public override void Exit(bool isResimulating)
    {
        if (!isResimulating)
        {
            abilitySystem.animator.SetBool("isDancing", false);
            abilitySystem.animator.applyRootMotion = false;
        }
    }

    public override PlayerStateType HandleInput(InputInfo input, bool isServer)
    {
        base.HandleInput(input, isServer);

        if (input.PressedKeys.Count > 0 && (input.PressedKeys.Count > 1 || !input.IsKeyPressed(KeyCode.V)))
            return PlayerStateType.Idle;

        return this.Type;
    }
}
