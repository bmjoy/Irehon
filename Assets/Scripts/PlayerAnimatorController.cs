using UnityEngine;
using Mirror;

public class PlayerAnimatorController : EntityAnimatorController
{
    [SerializeField]
    protected Transform shoulderLookTarget;
    protected Transform head;
    protected Player player;

    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();
        head = animator.GetBoneTransform(HumanBodyBones.Head);
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        head.LookAt(shoulderLookTarget);
    }

    public void SetFallingState(bool isFalling)
    {
        animator.SetBool("Falling", isFalling);
    }

    public void PlayJump()
    {
        animator.SetTrigger("Jump");
    }

    protected virtual void Sprint(bool isSprint)
    {
        animator.SetBool("Sprint", isSprint);
    }

    protected void SetSprintState(bool isSprint)
    {
        animator.SetBool("Sprint", isSprint);
        animator.SetBool("Walking", !isSprint);
    }

    public virtual void ApplyAnimatorInput(PlayerController.InputState input)
    {
        Vector2 moveVerticals = input.GetMoveVector();

        if (moveVerticals != Vector2.zero)
        {
            if (input.SprintKeyDown && moveVerticals.x == 0 && moveVerticals.y > 0)
                SetSprintState(true);
            else
                SetSprintState(false);
        }
        else
        {
            animator.SetBool("Sprint", false);
            animator.SetBool("Walking", false);
        }
        animator.SetFloat("xMove", moveVerticals.x);
        animator.SetFloat("zMove", moveVerticals.y);
    }
}
