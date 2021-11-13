using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerRotatableState : PlayerState
{
    protected PlayerRotatableState(Player player) : base(player)
    {
        playerBonesLinks = player.GetComponent<PlayerBonesLinks>();
    }

    private PlayerBonesLinks playerBonesLinks;

    public override bool CanRotateCamera => true;

    public override PlayerStateType HandleInput(InputInfo input, bool isServer)
    {
        if (isServer)
        {
            player.SetRotation(new Vector3(0, input.CameraRotation.y, 0));
            playerBonesLinks.Shoulder.localRotation = Quaternion.Euler(input.CameraRotation.x, -3.5f, 0f);
        }
        return this.Type;
    }
}
