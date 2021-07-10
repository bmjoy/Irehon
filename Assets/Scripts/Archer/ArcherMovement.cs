using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherMovement : PlayerMovement
{
    protected IAbilitySystem abilitySystem;

    protected override void Awake()
    {
        base.Awake();
        abilitySystem = GetComponent<IAbilitySystem>();
    }

    public override void ProcessMovementInput(PlayerController.InputState input)
    {
        Vector2 moveVerticals = input.GetMoveVector();
        Vector3 moveVector = Vector3.zero;

        moveVector.x = moveVerticals.x;
        moveVector.z = moveVerticals.y;

        moveVector = Vector3.ClampMagnitude(moveVector, 1);

        if (abilitySystem.IsAbilityCasting())
            moveVector /= 2.5f;

        //Walking side slowness
        moveVector = moveVector.x != 0 || moveVector.z < 0 ? moveVector / 1.5f : moveVector;

        if (!abilitySystem.IsAbilityCasting())
            moveVector = input.SprintKeyDown ? moveVector.x == 0 && moveVector.z > 0 ? moveVector * 2f : moveVector : moveVector;

        transform.rotation = Quaternion.Euler(0, input.currentRotation, 0);
        transform.Translate(moveVector * MOVEMENT_SPEED);
        ApplyAnimatorInput(input);
    }

    protected override void ApplyAnimatorInput(PlayerController.InputState input)
    {
        Vector2 moveVerticals = input.GetMoveVector();

        if (input.SprintKeyDown && !abilitySystem.IsAbilityCasting())
            animator.SetBool("Sprint", moveVerticals.y > 0);
        else
            animator.SetBool("Sprint", false);
        if (moveVerticals != Vector2.zero)
        {
            animator.SetBool("Walking", true);
        }
        else
        {
            animator.SetBool("Walking", false);
        }
        animator.SetFloat("xMove", moveVerticals.x);
        animator.SetFloat("zMove", moveVerticals.y);
    }
}
