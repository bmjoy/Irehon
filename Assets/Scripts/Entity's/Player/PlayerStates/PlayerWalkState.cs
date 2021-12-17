using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerWalkState : PlayerRotatableState
{
    public PlayerWalkState(Player player) : base(player)
    {
        animator = player.GetComponent<Animator>();
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    private Animator animator;
    private PlayerMovement playerMovement;

    public override bool CanRotateCamera => true;
    public override float MovementSpeed => 1;
    public override bool CanInteract => true;
    public override PlayerStateType Type => PlayerStateType.Walk;

    public override void Enter(bool isResimulating)
    {
        animator.SetBool("Walking", true);
    }

    public override void Exit(bool isResimulating)
    {
        animator.SetBool("Walking", false);
        animator.SetFloat("xMove", 0);
        animator.SetFloat("zMove", 0);
    }

    public override PlayerStateType HandleInput(InputInfo input, bool isServer)
    {
        base.HandleInput(input, isServer);

        if (isServer)
            abilitySystem.SendAbilityKeyStatus(input.IsKeyPressed(abilitySystem.ListeningKey), input.TargetPoint);

        if (input.IsKeyPressed(KeyCode.Space))
            return PlayerStateType.Jump;

        if (isServer && input.interactionTarget != null)
            playerInteracter.InterractAttemp(input.interactionTarget);

        if (input.GetMoveVector() == Vector2.zero)
            return PlayerStateType.Idle;

        if (input.IsKeyPressed(KeyCode.LeftShift) && input.GetMoveVector().x == 0 && input.GetMoveVector().y > 0)
            return PlayerStateType.Run;

        playerMovement.Move(input.GetMoveVector(), abilitySystem.IsAbilityCasting() ? MovementSpeed / 3 : MovementSpeed);
        animator.SetFloat("xMove", input.GetMoveVector().x);
        animator.SetFloat("zMove", input.GetMoveVector().y);

        return this.Type;
    }
}
