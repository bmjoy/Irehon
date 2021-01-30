using UnityEngine;
using Mirror;

public class PlayerAnimatorController : AnimatorController
{
    [SerializeField]
    protected Transform shoulderLookTarget;
    protected Transform head;

    protected override void Start()
    {
        base.Start();
        head = animator.GetBoneTransform(HumanBodyBones.Head);
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        head.LookAt(shoulderLookTarget);
    }

    public void ResetMovementAnimation()
    {
        animator.SetFloat("xMove", 0);
        animator.SetFloat("zMove", 0);
        animator.SetBool("Walking", false);

        SendedResetMovementAnimation();
    }

    public void ApplyAnimatorInput(PlayerController.InputState input)
    {
        Vector2 moveVerticals = input.GetMoveVector();

        if (moveVerticals != Vector2.zero)
            animator.SetBool("Walking", true);
        else
            animator.SetBool("Walking", false);
        animator.SetFloat("xMove", moveVerticals.x);
        animator.SetFloat("zMove", moveVerticals.y);
    }

    [Command]
    private void SendedResetMovementAnimation()
    {
        if (isLocalPlayer)
            return;
        animator.SetFloat("xMove", 0);
        animator.SetFloat("zMove", 0);
        animator.SetBool("Walking", false);
    }
}
