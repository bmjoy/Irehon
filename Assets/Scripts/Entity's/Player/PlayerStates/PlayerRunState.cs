using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunState : PlayerRotatableState
{
    public PlayerRunState(Player player) : base(player)
    {
        animator = player.GetComponent<Animator>();
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    private Animator animator;
    private PlayerMovement playerMovement;

    public override float MovementSpeed => 2;
    public override PlayerStateType Type => PlayerStateType.Run;
    public override bool CanInteract => false;

    public override void Enter()
    {
        animator.SetBool("Sprint", true);
        animator.SetBool("Walking", true);
    }

    public override void Exit()
    {
        animator.SetBool("Sprint", false);
        animator.SetBool("Walking", false);
        animator.SetFloat("xMove", 0);
        animator.SetFloat("zMove", 0);
    }

    public override PlayerState HandleInput(InputInfo input, bool isServer)
    {
        base.HandleInput(input, isServer);

        if (input.IsKeyPressed(KeyCode.Space))
            return new PlayerJumpingState(player);

        if (input.GetMoveVector() == Vector2.zero)
            return new PlayerIdleState(player);

        if (!input.IsKeyPressed(KeyCode.LeftShift) || input.GetMoveVector().x != 0 || input.GetMoveVector().y <= 0)
            return new PlayerWalkState(player);

        playerMovement.Move(input.GetMoveVector(), MovementSpeed);
        
        animator.SetFloat("xMove", input.GetMoveVector().x);
        animator.SetFloat("zMove", input.GetMoveVector().y);

        return this;
    }
}
