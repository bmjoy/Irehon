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
    [SerializeField]
    protected AudioSource stepSoundSource;
    protected Rigidbody rigidBody;
    protected Animator animator;
    protected bool isGrounded;

    protected virtual void Awake()
    {
        isGrounded = true;
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

    public virtual void ProcessMovementInput(PlayerController.InputState input)
    {
        Vector2 moveVerticals = input.GetMoveVector();
        Vector3 moveVector = Vector3.zero;

        moveVector.x = moveVerticals.x;
        moveVector.z = moveVerticals.y;

        moveVector = Vector3.ClampMagnitude(moveVector, 1);

        //Walking side slowness
        moveVector = moveVector.x != 0 || moveVector.z < 0 ? moveVector / 1.5f : moveVector;

        moveVector = input.SprintKeyDown && moveVector.x == 0 && moveVector.z > 0 ? moveVector * 1.4f : moveVector;

        transform.rotation = Quaternion.Euler(0, input.currentRotation, 0);
        transform.Translate(moveVector * MOVEMENT_SPEED);

        ApplyAnimatorInput(input);
    }

    protected virtual void ApplyAnimatorInput(PlayerController.InputState input)
    {
        Vector2 moveVerticals = input.GetMoveVector();

        animator.SetBool("Sprint", input.SprintKeyDown);
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

    public void StepSound()
    {
        stepSoundSource.Play();
    }
}
