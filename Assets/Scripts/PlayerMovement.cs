using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    private const float MOVEMENT_SPEED = 0.1f;
    private const float JUMP_FORCE = 7f;

    private bool isGrounded = true;

    private Animator animator;
    private Rigidbody rigidBody;

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Floor")
        {
            animator.SetBool("Falling", true);
            isGrounded = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Floor")
        {
            animator.SetBool("Falling", false);
            isGrounded = true;
        }
    }
    private void Start()
    {
        animator = GetComponent<Animator>();

        rigidBody = GetComponent<Rigidbody>();
    }

    public void ApplyTransformInput(PlayerController.InputState input)
    {
        Physics.autoSimulation = false;
        Vector2 moveVerticals = input.GetMoveVector();
        Vector3 moveVector = Vector3.zero;

        moveVector.x = moveVerticals.x;
        moveVector.z = moveVerticals.y;

        moveVector.z = moveVector.z < 0 ? moveVector.z /= 2.2f : moveVector.z;
        moveVector.x = moveVector.x != 0 ? moveVector.x /= 1.7f : 0;

        transform.rotation = Quaternion.Euler(0, input.currentRotation, 0);
        transform.Translate(moveVector * MOVEMENT_SPEED);
        Physics.Simulate(Time.fixedDeltaTime);
        Physics.autoSimulation = true;
    }

    public void ApplyAnimatorInput(PlayerController.InputState input)
    {
        Vector2 moveVerticals = input.GetMoveVector();

        if (moveVerticals != Vector2.zero)
            animator.SetBool("Walking", true);
        else
            animator.SetBool("Walking", false);
        animator.SetFloat("xMove", moveVerticals.x);
        animator.SetFloat("zMove", moveVerticals.y);
    }

    public void Jump()
    {
        if (isGrounded && rigidBody.velocity.y < 1)
        {
            rigidBody.AddForce(Vector3.up * JUMP_FORCE, ForceMode.Impulse);

            animator.SetTrigger("Jump");

            SendJump();
        }
    }

    [Command]
    private void SendJump()
    {
        if (isLocalPlayer)
            return;

        if (isGrounded && rigidBody.velocity.y < 1)
        {
            rigidBody.AddForce(Vector3.up * JUMP_FORCE, ForceMode.Impulse);
            JumpRpc();
            animator.SetTrigger("Jump");
        }
    }

    [TargetRpc]
    private void JumpRpc()
    {
        if (isLocalPlayer)
            return;
        rigidBody.AddForce(Vector3.up * JUMP_FORCE, ForceMode.Impulse);
    }
}
