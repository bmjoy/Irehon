using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    protected const float MOVEMENT_SPEED = 0.125f;
    protected const float JUMP_FORCE = 7f;

    protected bool isGrounded = true;

    protected Animator animator;
    protected Rigidbody rigidBody;
    protected PlayerController controller;

    protected void OnTriggerExit(Collider other)
    {
        //if (other.tag == "Floor")
        {
            animator.SetBool("Falling", true);
            isGrounded = false;
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
        //if (other.tag == "Floor")
        {
            animator.SetBool("Falling", false);
            isGrounded = true;
        }
    }

    [Command]
    private void SendJump()
    {
        if (isLocalPlayer)
            return;

        if (isGrounded && rigidBody.velocity.y < 1)
        {
            isGrounded = false;

            rigidBody.AddForce(Vector3.up * JUMP_FORCE, ForceMode.Impulse);
            JumpRpc();
            animator.SetTrigger("Jump");
        }
    }

    [ClientRpc(excludeOwner = true)]
    private void JumpRpc()
    {
        animator.SetTrigger("Jump");
        rigidBody.AddForce(Vector3.up * JUMP_FORCE, ForceMode.Impulse);
    }
    public virtual void ApplyTransformInput(PlayerController.InputState input)
    {
        Vector2 moveVerticals = input.GetMoveVector();
        Vector3 moveVector = Vector3.zero;

        moveVector.x = moveVerticals.x;
        moveVector.z = moveVerticals.y;

        //Walking side slowness
        moveVector.z = moveVector.z < 0 ? moveVector.z /= 2.2f : moveVector.z;
       
        //Back side slowness
        moveVector.x = moveVector.x != 0 ? moveVector.x /= 1.4f : 0;

        transform.rotation = Quaternion.Euler(0, input.currentRotation, 0);
        transform.Translate(moveVector * MOVEMENT_SPEED);
    }

    public void Jump()
    {
        if (isGrounded && rigidBody.velocity.y < 1)
        {
            isGrounded = false;

            rigidBody.AddForce(Vector3.up * JUMP_FORCE, ForceMode.Impulse);

            animator.SetTrigger("Jump");

            SendJump();
        }
    }

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
        rigidBody = GetComponent<Rigidbody>();
    }
}
