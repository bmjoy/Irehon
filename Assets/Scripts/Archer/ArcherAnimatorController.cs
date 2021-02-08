using UnityEngine;
using Mirror;

public class ArcherAnimatorController : PlayerAnimatorController
{
    [SerializeField]
    private Vector3 aimingOffset;
    [SyncVar]
    private bool isAiming;
    private const float POST_AIM_ROTATION_DURATION = 0.14f;
    private Transform chest;
    private float postAimRotation = 0;

    protected override void Awake()
    {
        base.Awake();
        head = animator.GetBoneTransform(HumanBodyBones.Head);
        chest = animator.GetBoneTransform(HumanBodyBones.Chest);
}

    public void StartAiming()
    {
        isAiming = true;
        if (!isServerOnly)
            CameraController.instance.EnableAimCamera();
        animator.ResetTrigger("Shoot");
        animator.SetBool("Aiming", true);
    }

    public void StopAiming()
    {
        isAiming = false;
        postAimRotation = POST_AIM_ROTATION_DURATION;
        if (!isServerOnly)
            CameraController.instance.DisableAimCamera();
        animator.SetBool("Aiming", false);
        animator.ResetTrigger("Shoot");
    }

    protected override void Sprint(bool isSprint)
    {
        if (!isAiming)
            base.Sprint(isSprint);
    }

    public override void ApplyAnimatorInput(PlayerController.InputState input)
    {
        Vector2 moveVerticals = input.GetMoveVector();

        if (moveVerticals != Vector2.zero)
        {
            if (input.SprintKeyDown && moveVerticals.x == 0 && moveVerticals.y > 0 && !isAiming)
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

    public void PlayShootAnimation()
    {
        animator.SetTrigger("Shoot");
        if (isServer)
            PlayShootAnimationOnOthers();
    }

    [ClientRpc(excludeOwner = true)]
    public void PlayShootAnimationOnOthers()
    {
        animator.SetTrigger("Shoot");
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        if (isAiming || postAimRotation > 0)
        {
            postAimRotation -= Time.deltaTime;
            chest.LookAt(shoulderLookTarget);
            chest.rotation *= Quaternion.Euler(aimingOffset);
        }
    }

    public override void ResetTriggers()
    {
        base.ResetTriggers();
        animator.ResetTrigger("Shoot");
    }

    public override void ResetAnimator()
    {
        base.ResetAnimator();
        animator.SetBool("Aiming", false);
    }
}
