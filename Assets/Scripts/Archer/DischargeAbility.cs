using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DischargeAbility : AbilityBase
{
    public override int Id => id;
    private int id = 1;
    private ParticleSystem chargeParticles;
    [SerializeField]
    private GameObject aoeAbilityPrefab;
    private float realCooldown;
    private AoeTargetMark targetMark;
    private DischargeAoeArrows aoeAbility;
    private Animator animator;
    private bool targeting;
    private PlayerController controller;
    private Vector3 targetMarkPos;
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip releaseSound;
    private Transform head;

    public override void AbilityInit(AbilitySystem abilitySystem)
    {
        base.AbilityInit(abilitySystem);
        realCooldown = cooldownTime;
        cooldownTime = 0;
        controller = abilitySystem.CharController;
        animator = abilitySystem.AnimatorComponent;
        targetMark = abilitySystem.AOETargetMark;
        audioSource = abilitySystem.AudioSource;
        head = animator.GetBoneTransform(HumanBodyBones.Head);
        chargeParticles = Instantiate(aoeAbilityPrefab, abilitySystem.AbilityPoolObject.transform)
            .GetComponent<ParticleSystem>();
        aoeAbility = chargeParticles.GetComponent<DischargeAoeArrows>();

        cooldownTime = 5f;
        triggerOnKey = KeyCode.F;
    }

    public override void AddAbilityCopy(GameObject holder)
    {
        base.AddAbilityCopy(holder);
        DischargeAbility abilityComponent = addingComponent as DischargeAbility;
        abilityComponent.releaseSound = releaseSound;
        abilityComponent.aoeAbilityPrefab = aoeAbilityPrefab;
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
        if (!isServer)
            return;

        RaycastHit hit;
        if (Vector3.Distance(target, head.position) < 25 &&
            Physics.Raycast(target + Vector3.up, Vector3.down, out hit, 2.5f, 1 << 11))
        {
            targetMarkPos = hit.point;
        }
        else
        {
            SetSpawnArrowPosition(Vector3.zero, false);
            targetMark.DisableTarget();
            AbilityEndEvent();
            return;
        }
        SetSpawnArrowPosition(targetMarkPos, true);
        StartCooldown(realCooldown);
        controller.BlockControll();
        animator.SetInteger("CurrentSkill", 2);
        animator.SetTrigger("Skill");
        chargeParticles.Play();
        audioSource.clip = releaseSound;
        audioSource.Play();
    }

    [ClientRpc]
    private void SetSpawnArrowPosition(Vector3 position, bool isTargetable)
    {
        if (isLocalPlayer)
        {
            targetMark.DisableTarget();
        }
        if (!isTargetable)
        {
            AbilityEndEvent();
            return;
        }

        targetMarkPos = position;
        StartCooldown(realCooldown);
        controller.BlockControll();
        animator.SetInteger("CurrentSkill", 2);
        animator.SetTrigger("Skill");
        chargeParticles.Play();
        audioSource.clip = releaseSound;
        audioSource.Play();
    }
}
