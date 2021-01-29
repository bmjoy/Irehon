using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherClass : ClassData
{
    protected Bow currentWeapon;
    private bool isAiming;

    PlayerController playerController;
    ArcherAnimatorController animatorController;
    Cinemachine.CinemachineImpulseSource impulseSource;



    public override void IntializeClass()
    {
        GetComponent<Animator>().runtimeAnimatorController = classAnimator;
        playerController = GetComponent<PlayerController>();
        currentWeapon = GetComponent<Bow>();
        animatorController = GetComponent<ArcherAnimatorController>();
        impulseSource = GetComponent<Cinemachine.CinemachineImpulseSource>();
        isAiming = false;
    }

    public override void LeftMouseButtonDown()
    {
        if (!currentWeapon.ReleaseProjectile(CameraController.instance.GetLookingTargetPosition()))
            return;
        animatorController.ShootAnimator();
        animatorController.StopAiming();
        currentWeapon.InterruptAiming();
        UIController.instance.EnableDefaultCrosshair();
        impulseSource.GenerateImpulse(CameraController.instance.transform.forward);
        isAiming = false;
    }
    public override void RightMouseButtonDown()
    {
        currentWeapon.StartAim();
        animatorController.StartAiming();
        UIController.instance.EnableTriangleCrosshair();
        isAiming = true;
    }
    public override void RightMouseButtonUp()
    {
        currentWeapon.InterruptAiming();
        animatorController.StopAiming();
        UIController.instance.EnableDefaultCrosshair();
        isAiming = false;
    }
    public override void LeftMouseButtonUp()
    {
        
    }

    public bool IsAiming() => isAiming;
}
