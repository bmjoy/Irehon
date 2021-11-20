using UnityEngine;
using Mirror;

public class PlayerMovement : MonoBehaviour
{
    private float baseMovementSpeed = 0.05f;

    private float gravity = 5f;

    public bool IsGrounded { private set; get; }

    [HideInInspector]
    public float yVelocity;

    private CharacterController characterController;
    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        Player player = GetComponent<Player>();
        if (player.isClient && !player.isLocalPlayer)
            characterController.enabled = false;
    }

    private void FixedUpdate()
    {
        yVelocity -= Time.fixedDeltaTime * gravity;
        if (yVelocity < -gravity)
            yVelocity = -gravity;
        characterController.Move(Vector3.up * yVelocity * Time.fixedDeltaTime);
        IsGrounded = characterController.isGrounded;
    }

    public void Move(Vector2 move, float speed)
    {
        Vector3 moveVector;

        moveVector = move.x * transform.right + move.y * transform.forward;

        moveVector.y = 0;

        moveVector = Vector3.ClampMagnitude(moveVector, 1);

        moveVector.y = 0;

        //Walking side slowness
        moveVector = move.x != 0 || move.y < 0 ? moveVector / 1.5f : moveVector;

        characterController.Move(moveVector * baseMovementSpeed * speed);
    }
}
