using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallState : PlayerRotatableState
{
    public PlayerFallState(Player player) : base(player)
    {
        animator = player.GetComponent<Animator>();
        playerGroundDetector = player.GetComponent<PlayerGroundDetector>();
        rigidBody = player.GetComponent<Rigidbody>();
    }

    private const float velocityIncreasing = 8f;

    private Animator animator;
    private Rigidbody rigidBody;
    private PlayerGroundDetector playerGroundDetector;

    public override bool CanRotateCamera => true;
    public override float MovementSpeed => 0;
    public override PlayerStateType Type => PlayerStateType.Fall;
    public override bool CanInteract => false;
    public override void Enter()
    {
        Debug.Log("Enter in dfalling state");
        animator.SetBool("Falling", true);
        playerGroundDetector.OnLand.AddListener(ChangeState);
    }

    public override void Exit()
    {
        Debug.Log("Exit from dfalling state");
        animator.SetBool("Falling", false);
        playerGroundDetector.OnLand.RemoveListener(ChangeState);
    }

    private void ChangeState()
    {
        player.GetComponent<PlayerStateMachine>().ChangePlayerState(new PlayerIdleState(player));
    }

    public override void Update()
    {
        Debug.Log("Fall");
        rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y - (velocityIncreasing * Time.fixedDeltaTime), rigidBody.velocity.z);
    }

    public override PlayerState HandleInput(InputInfo input, bool isServer)
    {
        return this;
    }
}
