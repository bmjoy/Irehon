using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    protected const float MOVEMENT_SPEED = 0.125f;
    protected const float JUMP_FORCE = 9f;
    [SerializeField]
    protected float velocityJumpIncreasing;
    protected Rigidbody rigidBody;
    protected PlayerController controller;
    protected Player player;

    protected virtual void Awake()
    {
        player = GetComponent<Player>();
        controller = GetComponent<PlayerController>();
        rigidBody = GetComponent<Rigidbody>();
    }

    public virtual void ApplyTransformInput(PlayerController.InputState input)
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
    }

    public void Jump()
    {
        rigidBody.AddForce(Vector3.up * JUMP_FORCE, ForceMode.Impulse);
    }

    public void IncreaseJumpGravityForce()
    {
        if (rigidBody.velocity.y < 0)
            rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y - (velocityJumpIncreasing * Time.fixedDeltaTime), rigidBody.velocity.z);
    }
}
