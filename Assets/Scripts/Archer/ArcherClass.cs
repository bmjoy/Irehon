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
        if (!currentWeapon.ReleaseProjectile(CameraController.instance.GetLookingTargetPosition()))
            return;
        archerController.Shoot();
        archerController.StopAim();
        currentWeapon.InterruptAiming();
        UIController.instance.EnableDefaultCrosshair();
    }
    public override void RightMouseButtonDown()
    {
        currentWeapon.StartAim();
        archerController.StartAim();
        UIController.instance.EnableTriangleCrosshair();
    }
    public override void RightMouseButtonUp()
    {
        currentWeapon.InterruptAiming();
        archerController.StopAim();
        UIController.instance.EnableDefaultCrosshair();
    }
    public override void LeftMouseButtonUp()
    {
    }
}
