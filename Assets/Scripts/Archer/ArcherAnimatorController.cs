using UnityEngine;
using Mirror;
using System.Collections;

public class ArcherAnimatorController : PlayerAnimatorController
{
    [SerializeField]
    private Vector3 aimingOffset;
    [SerializeField]
    private ParticleSystem aimingParticles;
    private ArcherController controller;
    [SyncVar]
    private bool isAiming;
    private float aimingDuration;
    private const float POST_AIM_ROTATION_DURATION = 0.14f;
    private Transform chest;
    private float postAimRotation = 0;

    protected override void Awake()
    {
        base.Awake();
        controller = GetComponent<ArcherController>();
        head = animator.GetBoneTransform(HumanBodyBones.Head);
        chest = animator.GetBoneTransform(HumanBodyBones.Chest);
}

    public void StartAiming()
    {
        aimingParticles.gameObject.SetActive(true);
        aimingParticles.Play();
        isAiming = true;
        animator.ResetTrigger("Shoot");
        animator.SetTrigger("DrawArrow");
        animator.SetBool("Aiming", true);
    }

    public void StartLookAtTarget()
    {
        isAiming = true;
    }

    public void StopLookAtTarget()
    {
        isAiming = false;
    }

    public void StopAiming()
    {
        isAiming = false;
        aimingParticles.gameObject.SetActive(false);
        postAimRotation = POST_AIM_ROTATION_DURATION;
        if (!isServerOnly)
            CameraController.instance.DisableAimCamera();
        animator.SetBool("Aiming", false);
        animator.ResetTrigger("DrawArrow");
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
        aimingDuration = 0.8f;
        if (isServer)
            PlayShootAnimationOnOthers();
    }

    [ClientRpc(excludeOwner = true)]
    public void PlayShootAnimationOnOthers()
    {
        animator.SetTrigger("Shoot");
    }

    //protected override void LateUpdate()
    //{
    //    base.LateUpdate();
    //    if (isAiming || postAimRotation > 0)
    //    {
    //        postAimRotation -= Time.deltaTime;
    //        chest.LookAt(shoulderLookTarget);
    //        chest.rotation *= Quaternion.Euler(aimingOffset);
    //        head.LookAt(shoulderLookTarget);
    //    }
    //    if (isAiming || aimingDuration > 0)
    //    {
    //        aimingDuration -= Time.deltaTime;
    //        controller.StartAimingMovement();
    //        animator.SetBool("Aiming", true);
    //    }
    //    else
    //    {
    //        animator.SetBool("Aiming", false);
    //        controller.StopAimingMovement();
    //    }
    //}

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

    private IEnumerable AimingAnimation()
    {
        while (aimingDuration > 0)
        {
            aimingDuration -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        yield break;
    }
}
