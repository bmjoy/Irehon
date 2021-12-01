using UnityEngine;
using Mirror;

public class PlayerMovement : MonoBehaviour
{
    private float baseMovementSpeed = 0.05f;

    [SerializeField]
    private float gravity = 5f;

    private float stepAngle;

    public bool IsGrounded { private set; get; }
    [SerializeField]
    private float slideSpeed = 1f;

    [HideInInspector]
    public float yVelocity { set { velocity.y = value; } }

    private Vector3 velocity;
    private RaycastHit slopeHit;

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
        if (IsSteepSlope())
            StepSlopeMove();
        else
        {
            velocity.x = 0;
            velocity.z = 0;
        }
        velocity.y -= gravity * Time.fixedDeltaTime;
        characterController.Move(velocity * Time.fixedDeltaTime);
        IsGrounded = characterController.isGrounded;
        if (IsGrounded)
            velocity.y = 0;
    }

    private void StepSlopeMove()
    {
        Vector3 slopeDirection = Vector3.up - slopeHit.normal * Vector3.Dot(Vector3.up, slopeHit.normal);
        float slideSpeed = baseMovementSpeed + this.slideSpeed * Time.fixedDeltaTime;

        velocity = slopeDirection * -slideSpeed;
        velocity.y = velocity.y - slopeHit.point.y;
    }

    private bool IsSteepSlope()
    {
        if (!IsGrounded)
            return false;

        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, characterController.height / 2 + 1))
        {
            stepAngle = Vector3.Angle(slopeHit.normal, Vector3.up);
            if (stepAngle > characterController.slopeLimit)
                return true;
        }
        return false;
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
