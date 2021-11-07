using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeathState : PlayerState
{
    public PlayerDeathState(Player player) : base(player)
    {
        playerBonesLinks = player.GetComponent<PlayerBonesLinks>();
    }

    private PlayerBonesLinks playerBonesLinks;

    public override bool CanTakeDamage => false;
    public override bool CanRotateCamera => false;
    public override PlayerStateType Type => PlayerStateType.Death;

    public override float MovementSpeed => 0;

    public override bool CanInteract => false;

    public override void Enter()
    {
        abilitySystem.AbilityInterrupt();
        UIController.i.HideStatusCanvas();
        playerInteracter.StopInterracting();
        player.HideModel();
    }

    public override void Exit()
    {
        player.ShowModel();
        UIController.i.ShowStatusCanvas();
    }

    public override PlayerStateType HandleInput(InputInfo input, bool isServer)
    {
        return this.Type;
    }
}
