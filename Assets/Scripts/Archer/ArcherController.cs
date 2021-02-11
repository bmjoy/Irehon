using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherController : PlayerController
{
    private bool isAiming;
    private bool aimingMovement;
    private new ArcherAnimatorController animator;
    private new ArcherClass currentClass;

    protected override void Start()
    {
        base.Start();
        currentClass = GetComponent<ArcherClass>();
        animator = GetComponent<ArcherAnimatorController>();
    }

    protected override void Update()
    {
        if (previousInput.SprintKeyDown)
            return;
        base.Update();
        if (!isLocalPlayer)
            return;
        if (!isControllAllow)
            return;
        if (Input.GetKeyDown(KeyCode.E))
        {
            currentClass.RecoilShot();
        }
    }

    public bool IsAiming() => isAiming;

    public bool IsAimingMovement() => aimingMovement;

    public void StartAim()
    {
        isAiming = true;
        StartAimingMovement();
        if (!isServerOnly)
            CameraController.instance.EnableAimCamera();
        animator.StartAiming();
    }

    public void StartAimingMovement()
    {
        aimingMovement = true;
    }

    public void StopAimingMovement()
    {
        aimingMovement = false;
    }

    public void StopAim()
    {
        isAiming = false;
        StopAimingMovement();
        animator.StopAiming();
    }

    public void Shoot()
    {
        animator.PlayShootAnimation();
    }

    public void Shoot(int skill)
    {
        if (skill != 0)
            animator.PlaySkillAnimation(skill);
        else
            animator.PlayShootAnimation();
    }
}
