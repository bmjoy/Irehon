using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherMovement : PlayerMovement
{
    ArcherClass archerClass;

    protected override void Awake()
    {
        base.Awake();
        archerClass = GetComponent<ArcherClass>();
    }

    public override void ApplyTransformInput(PlayerController.InputState input)
    {
        Vector2 moveVerticals = input.GetMoveVector();
        Vector3 moveVector = Vector3.zero;

        moveVector.x = moveVerticals.x;
        moveVector.z = moveVerticals.y;

        if (archerClass.IsAiming())
            moveVector /= 2.5f;

        //Walking side slowness
        moveVector.z = moveVector.z < 0 ? moveVector.z /= 2.2f : moveVector.z;

        //Walking forward speed bost and if it's with side then slow
        if (!archerClass.IsAiming())
            moveVector.z = moveVector.z > 0 && moveVector.x != 0 ? moveVector.z /= 1.5f : moveVector.z * 1.5f;
        //Back side slowness
        moveVector.x = moveVector.x != 0 ? moveVector.x /= 1.4f : 0;

        transform.rotation = Quaternion.Euler(0, input.currentRotation, 0);
        transform.Translate(moveVector * MOVEMENT_SPEED);
    }
}
