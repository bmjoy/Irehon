using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallState : PlayerRotatableState
{
    public PlayerFallState(Player player) : base(player)
    {
        animator = player.GetComponent<Animator>();
        playerGroundDetector = player.GetComponent<PlayerGroundDetector>();
        rigidBody = player.GetComponent<Rigidbody>();
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    private const float gravity = 5f;

    private PlayerMovement playerMovement;
    private Animator animator;
    private Rigidbody rigidBody;
    private PlayerGroundDetector playerGroundDetector;

    public override bool CanRotateCamera => true;
    public override float MovementSpeed => 0.8f;
    public override PlayerStateType Type => PlayerStateType.Fall;
    public override bool CanInteract => false;
    public override void Enter()
    {
        animator.SetBool("Falling", true);
    }

    public override void Exit()
    {
        animator.SetBool("Falling", false);
    }

    public override void Update()
    {
        rigidBody.velocity = new Vector3(0, rigidBody.velocity.y - (gravity * Time.fixedDeltaTime), 0);
    }

    public override PlayerStateType HandleInput(InputInfo input, bool isServer)
    {
        base.HandleInput(input, isServer);

        playerMovement.Move(input.GetMoveVector(), MovementSpeed);
        
        if (playerGroundDetector.isGrounded)
            return PlayerStateType.Idle;
        else
            return this.Type;
    }
}
