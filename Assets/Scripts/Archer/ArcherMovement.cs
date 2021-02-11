using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherMovement : PlayerMovement
{
    public bool IsAiming;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void ProcessMovementInput(PlayerController.InputState input)
    {
        Vector2 moveVerticals = input.GetMoveVector();
        Vector3 moveVector = Vector3.zero;

        moveVector.x = moveVerticals.x;
        moveVector.z = moveVerticals.y;

        moveVector = Vector3.ClampMagnitude(moveVector, 1);

        if (IsAiming)
            moveVector /= 2.5f;

        //Walking side slowness
        moveVector = moveVector.x != 0 || moveVector.z < 0 ? moveVector / 1.5f : moveVector;

        if (!IsAiming)
            moveVector = input.SprintKeyDown && moveVector.x == 0 && moveVector.z > 0 ? moveVector * 1.4f : moveVector;

        transform.rotation = Quaternion.Euler(0, input.currentRotation, 0);
        transform.Translate(moveVector * MOVEMENT_SPEED);
        ApplyAnimatorInput(input);
    }
}
