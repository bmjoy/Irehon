using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField]
    private float baseMovementSpeed = 0.125f;

    private Rigidbody rigidBody;

    protected virtual void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    public void Move(Vector2 move, float speed)
    {
        Vector3 moveVector = Vector3.zero;

        moveVector.x = move.x;
        moveVector.z = move.y;

        moveVector = Vector3.ClampMagnitude(moveVector, 1);

        //Walking side slowness
        moveVector = moveVector.x != 0 || moveVector.z < 0 ? moveVector / 1.5f : moveVector;

        transform.Translate(moveVector * baseMovementSpeed * speed);
    }
}
