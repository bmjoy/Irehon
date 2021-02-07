using System.Collections.Generic;
using UnityEngine;

public class ArcherClass : ClassData
{
    protected Bow currentWeapon;
    protected ArcherController archerController;
    public override void IntializeClass()
    {
        GetComponent<Animator>().runtimeAnimatorController = classAnimator;
        archerController = GetComponent<ArcherController>();
        currentWeapon = GetComponent<Bow>();
    }

    public override void LeftMouseButtonDown()
    {
        if (archerController.IsAiming())
        {
            if (!currentWeapon.ReleaseProjectile(CameraController.instance.GetLookingTargetPosition()))
                return;
            archerController.Shoot();
            currentWeapon.SetCurrentArrowType(Bow.ArrowType.common);
            archerController.StopAim();
            currentWeapon.InterruptAiming();
            UIController.instance.EnableDefaultCrosshair();
        }
        else
        {
            archerController.Shoot();
        }
    }
    public override void RightMouseButtonDown()
    {
        currentWeapon.SetCurrentArrowType(Bow.ArrowType.snipe);
        currentWeapon.StartAim();
        archerController.StartAim();
        UIController.instance.EnableTriangleCrosshair();
    }
    public override void RightMouseButtonUp()
    {
        currentWeapon.SetCurrentArrowType(Bow.ArrowType.common);
        currentWeapon.InterruptAiming();
        archerController.StopAim();
        UIController.instance.EnableDefaultCrosshair();
    }
    public override void LeftMouseButtonUp()
    {

    }

    public override void AttackEvent()
    {
        currentWeapon.ReleaseProjectile(CameraController.instance.GetLookingTargetPosition());
    }
}
