using Irehon;
using UnityEngine;

public class PlayerJumpingState : PlayerRotatableState
{
    public const int JumpCost = 2500;
    public PlayerJumpingState(Player player) : base(player)
    {
        this.animator = player.GetComponent<Animator>();
        this.playerMovement = player.GetComponent<PlayerMovement>();
    }

    private const float jump_force = 3.4f;

    private PlayerMovement playerMovement;
    private Animator animator;

    public override float MovementSpeed => 2.2f;
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
        {
            return;
        }
        this.abilitySystem.AbilityInterrupt();
        this.playerInteracter.StopInterracting();
        if (this.playerMovement.IsGrounded)
        {
            player.staminaPoints -= JumpCost;
            this.playerMovement.yVelocity = jump_force;
            this.animator.SetTrigger("Jump");
        }
    }

    public override void Exit(bool isResimulating)
    {

    }
}
