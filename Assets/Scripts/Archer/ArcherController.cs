using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherController : PlayerController
{
    private bool isAiming;
    private new ArcherAnimatorController animator;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<ArcherAnimatorController>();
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
}
