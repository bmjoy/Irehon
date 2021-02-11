using System.Collections.Generic;
using UnityEngine;

public class ArcherClass : ClassData
{
    [SerializeField]
    private float skillRecoilPower = 9f;
    [SerializeField]
    private float skillRecoilCooldown = 9f;
    [SerializeField]
    private float recoilReset = 1f;
    [SerializeField]
    private ParticleSystem recoilStartParticle;
    [SerializeField]
    private ParticleSystem recoilReleaseParticle;
    private float currentRecoilCooldown = 0f;
    private enum Skill { None, RecoilShot}

    private Bow currentWeapon;
    private ArcherController archerController;
    private Skill playingSkill;

    private void FixedUpdate()
    {
        if (currentRecoilCooldown > 0)
            currentRecoilCooldown -= Time.fixedDeltaTime;
    }

    public override void IntializeClass()
    {
        GetComponent<Animator>().runtimeAnimatorController = classAnimator;
        archerController = GetComponent<ArcherController>();
        currentWeapon = GetComponent<Bow>();
    }

    public override void LeftMouseButtonDown()
    {
        if (playingSkill != Skill.None)
            return;
        if (archerController.IsAiming())
        {
            if (!currentWeapon.ReleaseProjectile(CameraController.instance.GetLookingTargetPosition()))
                return;
            archerController.Shoot();
            archerController.ResetState();
        }
        else
        {
            ResetClassState();
            archerController.Shoot();
        }
    }

    public override void RightMouseButtonDown()
    {
        if (playingSkill != Skill.None)
            return;
        currentWeapon.SetCurrentArrowType(Bow.ArrowType.Snipe);
        currentWeapon.StartAim();
        archerController.StartAim();
        UIController.instance.EnableTriangleCrosshair();
    }

    public override void RightMouseButtonUp()
    {
        archerController.ResetState();
    }

    public override void LeftMouseButtonUp()
    {

    }

    public override void AttackEvent()
    {
        if (!currentWeapon.ReleaseProjectile(CameraController.instance.GetLookingTargetPosition()))
            return;
        if (playingSkill != Skill.None)
        {
            switch (playingSkill)
            {
                case Skill.RecoilShot:
                    archerController.SetVelocity(Vector3.Normalize(-Vector3.forward + Vector3.up * CameraController.instance.GetPlayerYAxis() / 45) * skillRecoilPower);
                    Invoke(nameof(ResetVelocity), recoilReset);
                    archerController.ResetControll();
                    break;
            }
        }
        else
            archerController.ResetState();
    }

    public void RecoilShot()
    {
        if (currentRecoilCooldown > 0)
            return;
        recoilStartParticle.Play();
        archerController.ResetState();
        recoilReleaseParticle.Play();
        currentWeapon.SetCurrentArrowType(Bow.ArrowType.Recoil);
        playingSkill = Skill.RecoilShot;
        archerController.Shoot((int)Skill.RecoilShot);
        currentRecoilCooldown = skillRecoilCooldown;
        archerController.BlockControll();
    }

    public void ResetVelocity()
    {
        archerController.SetVelocity(Vector3.zero);
    }

    public override void ResetClassState()
    {
        playingSkill = Skill.None;
        currentWeapon.SetCurrentArrowType(Bow.ArrowType.Common);
        if (archerController.IsAiming())
        {
            currentWeapon.InterruptAiming();
            archerController.StopAim();
        }
        UIController.instance.EnableDefaultCrosshair();
    }
}
