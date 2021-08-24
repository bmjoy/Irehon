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

    public override float MovementSpeed => 0;

    public override bool CanInteract => false;

    public override void Enter()
    {
        playerBonesLinks.Model.gameObject.SetActive(false);
    }

    public override void Exit()
    {
        playerBonesLinks.Model.gameObject.SetActive(true);
    }

    public override PlayerState HandleInput(InputInfo input, bool isServer)
    {
        return this;
    }
}
