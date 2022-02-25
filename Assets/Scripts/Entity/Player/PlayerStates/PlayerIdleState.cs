using Irehon;
using UnityEngine;

public class PlayerIdleState : PlayerRotatableState
{
    public PlayerIdleState(Player player) : base(player)
    {
    }

    public override bool CanRotateCamera => true;

    public override float MovementSpeed => 0;

    public override bool CanInteract => true;
    public override PlayerStateType Type => PlayerStateType.Idle;
    public override void Enter(bool isResimulating)
    {
    }

    public override void Exit(bool isResimulating)
    {
    }

    public override PlayerStateType HandleInput(InputInfo input, bool isServer)
    {
        base.HandleInput(input, isServer);

        if (input.IsKeyPressed(KeyCode.Mouse1) && this.abilitySystem.playerWeaponEquipment.GetWeapon().GetType() != WeaponType.Bow)
        {
            return PlayerStateType.Block;
        }

        if (isServer)
        {
            this.abilitySystem.SendAbilityKeyStatus(input.IsKeyPressed(this.abilitySystem.ListeningKey), input.TargetPoint);
        }

        if (input.IsKeyPressed(KeyCode.Space) && player.staminaPoints > PlayerJumpingState.JumpCost)
        {
            return PlayerStateType.Jump;
        }

        if (input.IsKeyPressed(KeyCode.V))
        {
            return PlayerStateType.Dance;
        }

        if (isServer && input.interactionTarget != null)
        {
            this.playerInteracter.InterractAttemp(input.interactionTarget);
        }

        if (input.GetMoveVector() != Vector2.zero)
        {
            if (input.IsKeyPressed(KeyCode.LeftShift) && input.GetMoveVector().x == 0 && input.GetMoveVector().y > 0 && player.staminaPoints > PlayerRunState.MinimalStamina)
            {
                return PlayerStateType.Run;
            }

            return PlayerStateType.Walk;
        }
        return this.Type;
    }
}
