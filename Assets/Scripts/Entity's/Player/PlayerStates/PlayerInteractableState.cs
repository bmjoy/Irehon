using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerInteractableState : PlayerState
{
    protected PlayerInteractableState(Player player) : base(player)
    {
    }

    public override bool CanInteract => true;

    public override PlayerState HandleInput(InputInfo input, bool isServer)
    {
        if (isServer && input.IsKeyPressed(KeyCode.E))
            player.InterractAttempToServer(input.TargetPoint);
        return this;
    }
}
