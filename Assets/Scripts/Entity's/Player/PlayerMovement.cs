using UnityEngine;
using Mirror;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float baseMovementSpeed = 0.05f;

    [SerializeField]
    private float gravity = 5f;

    public bool IsGrounded { private set; get; }

    public float yVelocity;

    private CharacterController characterController;
    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void FixedUpdate()
    {
        yVelocity -= Time.fixedDeltaTime * gravity;
        if (yVelocity > 0)
            yVelocity -= Time.fixedDeltaTime * gravity * 0.25f;
        if (yVelocity < -gravity)
            yVelocity = -gravity;
        characterController.Move(Vector3.up * yVelocity * Time.deltaTime);
        IsGrounded = characterController.isGrounded;
    }

    public void Move(Vector2 move, float speed)
    {
        Vector3 moveVector;

        moveVector = move.x * transform.right + move.y * transform.forward;

        moveVector.y = 0;

        moveVector = Vector3.ClampMagnitude(moveVector, 1);

        //Walking side slowness
        moveVector = move.x != 0 || move.y < 0 ? moveVector / 1.5f : moveVector;

        characterController.Move(moveVector * baseMovementSpeed * speed);
    }
}
