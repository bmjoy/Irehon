using UnityEngine;

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
    public float yVelocity { set => this.velocity.y = value; }

    private Vector3 velocity;
    private RaycastHit slopeHit;

    private CharacterController characterController;
    private void Start()
    {
        this.characterController = this.GetComponent<CharacterController>();
        Player player = this.GetComponent<Player>();
        if (player.isClient && !player.isLocalPlayer)
        {
            this.characterController.enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (this.IsSteepSlope())
        {
            this.StepSlopeMove();
        }
        else
        {
            this.velocity.x = 0;
            this.velocity.z = 0;
        }
        this.velocity.y -= this.gravity * Time.fixedDeltaTime;
        this.characterController.Move(this.velocity * Time.fixedDeltaTime);
        this.IsGrounded = this.characterController.isGrounded;
        if (this.IsGrounded)
        {
            this.velocity.y = 0;
        }
    }

    private void StepSlopeMove()
    {
        Vector3 slopeDirection = Vector3.up - this.slopeHit.normal * Vector3.Dot(Vector3.up, this.slopeHit.normal);
        float slideSpeed = this.baseMovementSpeed + this.slideSpeed * Time.fixedDeltaTime;

        this.velocity = slopeDirection * -slideSpeed;
        this.velocity.y = this.velocity.y - this.slopeHit.point.y;
    }

    private bool IsSteepSlope()
    {
        if (!this.IsGrounded)
        {
            return false;
        }

        if (Physics.Raycast(this.transform.position, Vector3.down, out this.slopeHit, this.characterController.height / 2 + 1))
        {
            this.stepAngle = Vector3.Angle(this.slopeHit.normal, Vector3.up);
            if (this.stepAngle > this.characterController.slopeLimit)
            {
                return true;
            }
        }
        return false;
    }

    public void Move(Vector2 move, float speed)
    {
        Vector3 moveVector;

        moveVector = move.x * this.transform.right + move.y * this.transform.forward;

        moveVector.y = 0;

        moveVector = Vector3.ClampMagnitude(moveVector, 1);

        moveVector.y = 0;

        //Walking side slowness
        moveVector = move.x != 0 || move.y < 0 ? moveVector / 1.5f : moveVector;

        this.characterController.Move(moveVector * this.baseMovementSpeed * speed);
    }
}
