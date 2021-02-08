using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherController : PlayerController
{
    private bool isAiming;
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

    public void StartAim()
    {
        isAiming = true;
        animator.StartAiming();
    }

    public void StopAim()
    {
        isAiming = false;
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
