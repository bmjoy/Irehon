using Irehon;
using UnityEngine;

public class PlayerRunState : PlayerRotatableState
{
    public PlayerRunState(Player player) : base(player)
    {
        this.animator = player.GetComponent<Animator>();
        this.playerMovement = player.GetComponent<PlayerMovement>();
    }

    private Animator animator;
    private PlayerMovement playerMovement;

    public override float MovementSpeed => 2;
    public override PlayerStateType Type => PlayerStateType.Run;
    public override bool CanInteract => false;

    public override void Enter(bool isResimulating)
    {
        this.abilitySystem.AbilityInterrupt();
        this.animator.SetBool("Sprint", true);
        this.animator.SetBool("Walking", true);
    }

    public override void Exit(bool isResimulating)
    {
        this.animator.SetBool("Sprint", false);
        this.animator.SetBool("Walking", false);
        this.animator.SetFloat("xMove", 0);
        this.animator.SetFloat("zMove", 0);
    }

    public override PlayerStateType HandleInput(InputInfo input, bool isServer)
    {
        base.HandleInput(input, isServer);

        if (input.IsKeyPressed(KeyCode.Space))
        {
            return PlayerStateType.Jump;
        }

        if (input.GetMoveVector() == Vector2.zero)
        {
            return PlayerStateType.Idle;
        }

        if (!input.IsKeyPressed(KeyCode.LeftShift) || input.GetMoveVector().x != 0 || input.GetMoveVector().y <= 0)
        {
            return PlayerStateType.Walk;
        }

        this.playerMovement.Move(input.GetMoveVector(), this.MovementSpeed);

        this.animator.SetFloat("xMove", input.GetMoveVector().x);
        this.animator.SetFloat("zMove", input.GetMoveVector().y);

        return this.Type;
    }
}
