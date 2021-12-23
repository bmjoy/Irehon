using Irehon;
using UnityEngine;

public class PlayerWalkState : PlayerRotatableState
{
    public PlayerWalkState(Player player) : base(player)
    {
        this.animator = player.GetComponent<Animator>();
        this.playerMovement = player.GetComponent<PlayerMovement>();
    }

    private Animator animator;
    private PlayerMovement playerMovement;

    public override bool CanRotateCamera => true;
    public override float MovementSpeed => 1;
    public override bool CanInteract => true;
    public override PlayerStateType Type => PlayerStateType.Walk;

    public override void Enter(bool isResimulating)
    {
        this.animator.SetBool("Walking", true);
    }

    public override void Exit(bool isResimulating)
    {
        this.animator.SetBool("Walking", false);
        this.animator.SetFloat("xMove", 0);
        this.animator.SetFloat("zMove", 0);
    }

    public override PlayerStateType HandleInput(InputInfo input, bool isServer)
    {
        base.HandleInput(input, isServer);

        if (isServer)
        {
            this.abilitySystem.SendAbilityKeyStatus(input.IsKeyPressed(this.abilitySystem.ListeningKey), input.TargetPoint);
        }

        if (input.IsKeyPressed(KeyCode.Space))
        {
            return PlayerStateType.Jump;
        }

        if (isServer && input.interactionTarget != null)
        {
            this.playerInteracter.InterractAttemp(input.interactionTarget);
        }

        if (input.GetMoveVector() == Vector2.zero)
        {
            return PlayerStateType.Idle;
        }

        if (input.IsKeyPressed(KeyCode.LeftShift) && input.GetMoveVector().x == 0 && input.GetMoveVector().y > 0)
        {
            return PlayerStateType.Run;
        }

        this.playerMovement.Move(input.GetMoveVector(), this.abilitySystem.IsAbilityCasting() ? this.MovementSpeed / 3 : this.MovementSpeed);
        this.animator.SetFloat("xMove", input.GetMoveVector().x);
        this.animator.SetFloat("zMove", input.GetMoveVector().y);

        return this.Type;
    }
}
