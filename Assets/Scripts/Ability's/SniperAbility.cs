using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperAbility : AbilityBase
{
    private const float MAX_HOLDING_TIME = 1.7f;
    private const float MIN_HOLDING_TIME = 0.6f;

    private Bow bow;

    private Transform RightHand => boneLinks.RightHand;
    private Transform ShoulderForwardPoint => boneLinks.Shoulder;
    private GameObject Arrow => bow.Arrow;
    private AudioClip TenseSound => bow.TenseSound;
    private GameObject ArrowInHand => bow.ArrowInHand;
    private Transform BowStringBone => bow.BowStringBone;
    private ParticleSystem AimingParticles => bow.AimingParticles;

    private Vector3 aimingChestOffset = new Vector3(0, -5f, -86f);

    private Vector3 bowBoneStartPosition;
    private AudioSource abilityAudioSource;
    private Transform chestBone;
    private Animator animator;
    private Quiver quiver;
    private Player player;
    private PlayerMovement movement;

    private float holdingTime;
    private float additionalyChestOffsetTime;
    private bool aiming;



    public override void Setup(AbilitySystem abilitySystem)
    {
        base.Setup(abilitySystem);
        bow = weapon as Bow;
        bowBoneStartPosition = BowStringBone.localPosition;
        player = abilitySystem.PlayerComponent;
        abilityAudioSource = abilitySystem.AudioSource;
        movement = abilitySystem.GetComponent<PlayerMovement>();
        animator = abilitySystem.AnimatorComponent;
        chestBone = animator.GetBoneTransform(HumanBodyBones.Chest);
        quiver = new Quiver(abilitySystem.AbilityPoolObject.transform, player, 5, Arrow);
    }

    private void LateUpdate()
    {
        if (aiming)
            holdingTime += Time.deltaTime;
        if (holdingTime > MAX_HOLDING_TIME)
            holdingTime = MAX_HOLDING_TIME;
        if (holdingTime > MIN_HOLDING_TIME && isLocalPlayer)
            UIController.instance.ChangeTriangleAimSize(GetHoldingPowerPercent());
        if (aiming || additionalyChestOffsetTime > 0)
        {
            additionalyChestOffsetTime -= Time.deltaTime;
            chestBone.LookAt(ShoulderForwardPoint);
            chestBone.rotation *= Quaternion.Euler(aimingChestOffset);
        } 
    }

    private float GetHoldingPowerPercent()
    {
        return (holdingTime - MIN_HOLDING_TIME) / (MAX_HOLDING_TIME - MIN_HOLDING_TIME);
    }

    protected override void Ability(Vector3 target)
    {
        holdingTime = 0;
        additionalyChestOffsetTime = 5f;
        abilitySystem.BlockTrigger();
        animator.SetBool("Aiming", true);
        animator.SetBool("AimingMovement", true);
        currentAnimationEvent = () => StartCoroutine(ArrowInHandAnimation());
        
        if (isLocalPlayer)
        {
            UIController.instance.EnableTriangleCrosshair();
            CameraController.EnableAimCamera();
            UIController.instance.ChangeTriangleAimSize(holdingTime / MAX_HOLDING_TIME);
        }
    }

    private IEnumerator ArrowInHandAnimation()
    {
        abilityAudioSource.clip = TenseSound;
        abilityAudioSource.Play();
        aiming = true;
        ArrowInHand.SetActive(true);
        AimingParticles.Play();
        holdingTime = 0;
        while (aiming)
        {
            BowStringBone.position = RightHand.position;
            yield return null;
        }
    }

    private void ShootArrow(Vector3 target)
    {
        Arrow releasedArrow = quiver.GetArrowFromQuiver();
        float power = GetHoldingPowerPercent();
        releasedArrow.transform.position = ArrowInHand.transform.position;
        releasedArrow.transform.LookAt(target);
        releasedArrow.SetPower(GetHoldingPowerPercent());
        releasedArrow.TriggerReleaseEffect();
        releasedArrow.rigidBody.velocity = releasedArrow.transform.forward * (20 + GetHoldingPowerPercent() * 30);
        if (isLocalPlayer)
            CameraController.CreateShake(5, .1f);
    }

    protected override void StopHoldingAbility(Vector3 target)
    {
        if (abilityAudioSource.clip == TenseSound && abilityAudioSource.isPlaying)
            abilityAudioSource.Stop();
        if (aiming && holdingTime > MIN_HOLDING_TIME)
        {
            animator.SetTrigger("Shoot");
            ShootArrow(target);
            additionalyChestOffsetTime = 0.8f;
        }
        else
            additionalyChestOffsetTime = 0.05f;
        AimingParticles.Stop();
        BowStringBone.localPosition = bowBoneStartPosition;
        abilitySystem.AllowTrigger();
        ArrowInHand.SetActive(false);
        aiming = false;
        animator.SetBool("Aiming", false);
        animator.SetBool("AimingMovement", false);

        AbilityEnd();
        if (isLocalPlayer)
        {
            CameraController.DisableAimCamera();
            UIController.instance.EnableDefaultCrosshair();
        }
    }

    protected override void InterruptAbility()
    {
        if (isLocalPlayer)
            UIController.instance.EnableDefaultCrosshair();
        if (abilityAudioSource.clip == TenseSound && abilityAudioSource.isPlaying)
            abilityAudioSource.Stop();
        additionalyChestOffsetTime = 0;
        abilitySystem.AllowTrigger();
        BowStringBone.localPosition = bowBoneStartPosition;
        aiming = false;
        animator.ResetTrigger("Shoot");
        animator.SetBool("Aiming", false);
        animator.SetBool("AimingMovement", false);
    }

    private void OnDestroy()
    {
        Destroy(quiver);
    }
}
