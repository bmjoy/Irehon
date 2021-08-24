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

    public override void Enter()
    {
        animator.SetBool("Walking", true);
    }

    public override void Exit()
    {
        animator.SetBool("Walking", false);
        animator.SetFloat("xMove", 0);
        animator.SetFloat("zMove", 0);
    }

    public override PlayerState HandleInput(InputInfo input, bool isServer)
    {
        base.HandleInput(input, isServer);

        if (input.IsKeyPressed(KeyCode.Space))
            return new PlayerJumpingState(player);

        if (isServer && input.IsKeyPressed(KeyCode.E))
            player.InterractAttempToServer(input.TargetPoint);

        if (input.GetMoveVector() == Vector2.zero)
            return new PlayerIdleState(player);

        if (input.IsKeyPressed(KeyCode.LeftShift) && input.GetMoveVector().x == 0 && input.GetMoveVector().y > 0)
            return new PlayerRunState(player);

        playerMovement.Move(input.GetMoveVector(), MovementSpeed);
        animator.SetFloat("xMove", input.GetMoveVector().x);
        animator.SetFloat("zMove", input.GetMoveVector().y);

        return this;
    }
}
