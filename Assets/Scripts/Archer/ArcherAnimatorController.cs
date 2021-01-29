using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ArcherAnimatorController : PlayerAnimatorController
{
    [SerializeField]
    private Vector3 aimingOffset;
    [SyncVar]
    private bool aiming;
    private const float POST_AIM_ROTATION_DURATION = 0.14f;
    private Transform chest;
    private Transform head;
    private float postAimRotation = 0;

    protected override void Start()
    {
        base.Start();
        head = animator.GetBoneTransform(HumanBodyBones.Head);
        chest = animator.GetBoneTransform(HumanBodyBones.Chest);
}

    public void StartAiming()
    {
        aiming = true;
        if (!isServerOnly)
            CameraController.instance.EnableAimCamera();
        animator.SetBool("Aiming", true);
    }

    public void StopAiming()
    {
        aiming = false;
        postAimRotation = POST_AIM_ROTATION_DURATION;
        if (!isServerOnly)
            CameraController.instance.DisableAimCamera();
        animator.SetBool("Aiming", false);
    }

    public void ShootAnimator()
    {
        animator.SetTrigger("Shoot");
    }

    private void LateUpdate()
    {
        if (aiming || postAimRotation > 0)
        {
            postAimRotation -= Time.deltaTime;
            chest.LookAt(shoulderLookTarget);
            chest.rotation = chest.rotation * Quaternion.Euler(aimingOffset);
            head.LookAt(shoulderLookTarget);
        }
    }
}
