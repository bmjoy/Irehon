using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DischargeAbility : AbilityBase

{
    [SerializeField]
    private ParticleSystem chargeParticles;
    private float realCooldown;
    private AoeTargetMark targetMark;
    [SerializeField]
    private DischargeAoeArrows aoeAbility;
    private Animator animator;
    private bool targeting;
    private PlayerController controller;
    private Vector3 targetMarkPos;

    private new void Start()
    {
        base.Start();
        realCooldown = cooldownTime;
        cooldownTime = 0;
        controller = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        targetMark = GetComponent<AoeTargetMark>();
    }

    protected override void Ability(Vector3 target)
    {
        if (isLocalPlayer)
            targetMark.EnableTarget(0.5f, 25);
        targeting = true;
        currentAnimationEvent = StartSpawnArrows;
    }

    private void StartSpawnArrows()
    {
        aoeAbility.transform.position = targetMarkPos;
        aoeAbility.PlayArrowRain();
        controller.AllowControll();
        if (isLocalPlayer)
        {
            print("crate");
            CameraController.instance.CreateShake(5f, 0.5f);
            targetMark.DisableTarget();
        }
        AbilityEndEvent();
    }

    protected override void InterruptAbility()
    {
        if (isLocalPlayer)
            targetMark.DisableTarget();
        controller.AllowControll();
    }

    protected override void StopHoldingAbility(Vector3 target)
    {
        if (!targeting)
            return;
        targeting = false;
        if (isLocalPlayer)
        {
            if (targetMark.IsTargetable())
                targetMark.DisableTarget();
            else
            {
                targetMark.DisableTarget();
                AbilityEndEvent();
                return;
            }
        }

        RaycastHit hit;
        if (Physics.Raycast(target + Vector3.up, Vector3.down, out hit, 20, 1 << 11))
            targetMarkPos = hit.point;
        else
        {
            targetMark.DisableTarget();
            AbilityEndEvent();
            return;
        }

        StartCooldown(realCooldown);
        controller.BlockControll();
        animator.SetInteger("CurrentSkill", 2);
        animator.SetTrigger("Skill");
        chargeParticles.Play();
    }
}
