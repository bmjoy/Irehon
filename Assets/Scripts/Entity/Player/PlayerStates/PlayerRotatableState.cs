using Irehon;
using UnityEngine;

public abstract class PlayerRotatableState : PlayerState
{
    protected PlayerRotatableState(Player player) : base(player)
    {
        this.playerBonesLinks = player.GetComponent<PlayerBonesLinks>();
        this.characterController = player.GetComponent<CharacterController>();
    }

    private CharacterController characterController;
    private PlayerBonesLinks playerBonesLinks;

    public override bool CanRotateCamera => true;

    public override PlayerStateType HandleInput(InputInfo input, bool isServer)
    {
        if (isServer)
        {
            this.characterController.SetRotation(new Vector3(0, input.CameraRotation.y, 0));
            this.playerBonesLinks.Shoulder.localRotation = Quaternion.Euler(input.CameraRotation.x, -3.5f, 0f);
        }
        return this.Type;
    }
}
