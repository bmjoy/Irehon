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
        playerStateMachine = player.GetComponent<PlayerStateMachine>();
        playerGroundDetector = player.GetComponent<PlayerGroundDetector>();
    }

    private const float jump_force = 6.7f;

    private Rigidbody rigidBody;
    private PlayerGroundDetector playerGroundDetector;
    private PlayerMovement playerMovement;
    private PlayerStateMachine playerStateMachine;
    private Animator animator;

    public override float MovementSpeed => 1;
    public override PlayerStateType Type => PlayerStateType.Jump;
    public override bool CanInteract => false;

    public override PlayerStateType HandleInput(InputInfo input, bool isServer)
    {
        base.HandleInput(input, isServer);

        return PlayerStateType.Fall;
    }

    public override void Enter()
    {
        abilitySystem.AbilityInterrupt();
        playerInteracter.StopInterracting();
        if (playerGroundDetector.isGrounded)
        {
            rigidBody.velocity = new Vector3(rigidBody.velocity.x, jump_force, rigidBody.velocity.z);
            animator.SetTrigger("Jump");
        }
    }

    public override void Exit()
    {
        
    }
}
