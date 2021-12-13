using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallState : PlayerRotatableState
{
    public PlayerFallState(Player player) : base(player)
    {
        animator = player.GetComponent<Animator>();
        playerGroundDetector = player.GetComponent<PlayerGroundDetector>();
        playerMovement = player.GetComponent<PlayerMovement>();
        characterController = player.GetComponent<CharacterController>();
    }

    private const float gravity = 5f;

    private PlayerMovement playerMovement;
    private Animator animator;
    private CharacterController characterController;
    private PlayerGroundDetector playerGroundDetector;

    public override bool CanRotateCamera => true;
    public override float MovementSpeed => 1.45f;
    public override PlayerStateType Type => PlayerStateType.Fall;
    public override bool CanInteract => false;
    public override void Enter(bool isResimulating)
    {
        if (isResimulating)
            return;
        playerInteracter.StopInterracting();
        animator.SetBool("Falling", true);
    }

    public override void Exit(bool isResimulating)
    {
        if (isResimulating)
            return; 
        animator.SetBool("Falling", false);
    }

    public override void Update()
    {
    }

    public override PlayerStateType HandleInput(InputInfo input, bool isServer)
    {
        base.HandleInput(input, isServer);

        playerMovement.Move(input.GetMoveVector(), MovementSpeed);
        
        if (playerMovement.IsGrounded)
            return PlayerStateType.Idle;
        else
            return this.Type;
    }
}
