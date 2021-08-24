using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpingState : PlayerRotatableState
{
    public PlayerJumpingState(Player player) : base(player)
    {
        rigidBody = player.GetComponent<Rigidbody>();
        animator = player.GetComponent<Animator>();
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    private const float jump_force = 8f;

    private Rigidbody rigidBody;
    private PlayerMovement playerMovement;
    private Animator animator;

    public override float MovementSpeed => 0.5f;

    public override bool CanInteract => false;

    public override void Update()
    {
        if (rigidBody.velocity.y < 0)
            player.GetComponent<PlayerStateMachine>().ChangePlayerState(new PlayerFallState(player));
    }

    public override PlayerState HandleInput(InputInfo input, bool isServer)
    {
        base.HandleInput(input, isServer);

        playerMovement.Move(input.GetMoveVector(), MovementSpeed);
        animator.SetFloat("xMove", input.GetMoveVector().x);
        animator.SetFloat("zMove", input.GetMoveVector().y);

        return this;
    }

    public override void Enter()
    {
        rigidBody.velocity = new Vector3(rigidBody.velocity.x, jump_force, rigidBody.velocity.z);
        animator.SetTrigger("Jump");
    }

    public override void Exit()
    {

        animator.SetFloat("xMove", 0);
        animator.SetFloat("zMove", 0);
    }
}
