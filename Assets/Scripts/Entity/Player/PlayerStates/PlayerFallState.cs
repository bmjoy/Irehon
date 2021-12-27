using Irehon;
using UnityEngine;

public class PlayerFallState : PlayerRotatableState
{
    public PlayerFallState(Player player) : base(player)
    {
        this.animator = player.GetComponent<Animator>();
        this.playerMovement = player.GetComponent<PlayerMovement>();
    }

    private PlayerMovement playerMovement;
    private Animator animator;

    public override bool CanRotateCamera => true;
    public override float MovementSpeed => 1.45f;
    public override PlayerStateType Type => PlayerStateType.Fall;
    public override bool CanInteract => false;
    public override void Enter(bool isResimulating)
    {
        if (isResimulating)
        {
            return;
        }

        this.animator.SetBool("Falling", true);
    }

    public override void Exit(bool isResimulating)
    {
        if (isResimulating)
        {
            return;
        }

        this.animator.SetBool("Falling", false);
    }

    public override void Update()
    {
    }

    public override PlayerStateType HandleInput(InputInfo input, bool isServer)
    {
        base.HandleInput(input, isServer);

        this.playerMovement.Move(input.GetMoveVector(), this.MovementSpeed);

        if (this.playerMovement.IsGrounded)
        {
            return PlayerStateType.Idle;
        }
        else
        {
            return this.Type;
        }
    }
}
