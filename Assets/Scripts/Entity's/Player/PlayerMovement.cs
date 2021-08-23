using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour, IMovementBehaviour
{
    [SerializeField]
    protected float MOVEMENT_SPEED = 0.125f;
    [SerializeField]
    protected float JUMP_FORCE = 9f;
    [SerializeField]
    protected float velocityJumpIncreasing;
    protected Rigidbody rigidBody;
    protected AbilitySystem abilitySystem;
    protected Animator animator;
    protected bool isGrounded;

    protected virtual void Awake()
    {
        isGrounded = true;
        abilitySystem = GetComponent<AbilitySystem>();
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
    }
    public void FixedUpdate()
    {
        if (!isGrounded && rigidBody.velocity.y < 0)
            rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y - (velocityJumpIncreasing * Time.fixedDeltaTime), rigidBody.velocity.z);
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Walkable"))
        {
            animator.SetBool("Falling", false);
            isGrounded = true;
        }
    }

    protected void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Walkable"))
        {
            animator.SetBool("Falling", false);
            isGrounded = true;
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Walkable"))
        {
            animator.SetBool("Falling", true);
            isGrounded = false;
        }
    }

    public virtual void ProcessMovementInput(InputState input)
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

    protected virtual void ApplyAnimatorInput(InputState input)
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

    public void Jump()
    {
        rigidBody.velocity = new Vector3(rigidBody.velocity.x, JUMP_FORCE, rigidBody.velocity.z);
        animator.SetTrigger("Jump");
    }

    public bool IsCanJump()
    {
        return isGrounded && rigidBody.velocity.y < 1;
    }
}
