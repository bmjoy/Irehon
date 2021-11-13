using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpingState : PlayerRotatableState
{
    public PlayerJumpingState(Player player) : base(player)
    {
        animator = player.GetComponent<Animator>();
        playerMovement = player.GetComponent<PlayerMovement>();
        playerStateMachine = player.GetComponent<PlayerStateMachine>();
        characterController = player.GetComponent<CharacterController>();
    }

    private const float jump_force = 3.4f;

    private PlayerGroundDetector playerGroundDetector;
    private PlayerMovement playerMovement;
    private PlayerStateMachine playerStateMachine;
    private CharacterController characterController;
    private Animator animator;

    public override float MovementSpeed => 1;
    public override PlayerStateType Type => PlayerStateType.Jump;
    public override bool CanInteract => false;

    public override PlayerStateType HandleInput(InputInfo input, bool isServer)
    {
        base.HandleInput(input, isServer);

        return PlayerStateType.Fall;
    }

    public override void Enter(bool isResimulating)
    {
        if (isResimulating)
            return;

        abilitySystem.AbilityInterrupt();
        playerInteracter.StopInterracting();
        if (playerMovement.IsGrounded)
        {
            playerMovement.yVelocity = jump_force;
            animator.SetTrigger("Jump");
        }
    }

    public override void Exit(bool isResimulating)
    {
        
    }
}
