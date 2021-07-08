using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperAbility : AbilityBase
{
    private const float MAX_HOLDING_TIME = 1.7f;
    private const float MIN_HOLDING_TIME = 0.6f;

    private Bow bow;

    private Transform rightHand => prefabData.RightHand;
    private Transform shoulderForwardPoint => prefabData.ShoulderForwardPoint;
    private GameObject arrow => bow.Arrow;
    private AudioClip tenseSound => bow.TenseSound;
    private GameObject arrowInHand => bow.ArrowInHand;
    private Transform bowStringBone => bow.BowStringBone;
    private ParticleSystem aimingParticles => bow.AimingParticles;

    private Vector3 aimingChestOffset = new Vector3(0, -5f, -86f);

    private Vector3 bowBoneStartPosition;
    private AudioSource abilityAudioSource;
    private Transform chestBone;
    private Animator animator;
    private Quiver quiver;
    private Player player;
    private ArcherMovement movement;

    private float holdingTime;
    private float additionalyChestOffsetTime;
    private bool aiming;



    public override void Setup(AbilitySystem abilitySystem)
    {
        base.Setup(abilitySystem);
        bow = weapon as Bow;
        bowBoneStartPosition = bowStringBone.localPosition;
        player = abilitySystem.PlayerComponent;
        abilityAudioSource = abilitySystem.AudioSource;
        movement = abilitySystem.GetComponent<ArcherMovement>();
        animator = abilitySystem.AnimatorComponent;
        chestBone = animator.GetBoneTransform(HumanBodyBones.Chest);
        quiver = new Quiver(abilitySystem.AbilityPoolObject.transform, player, 5, arrow);
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
            chestBone.LookAt(shoulderForwardPoint);
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
        movement.IsAiming = true;
        animator.SetBool("Aiming", true);
        animator.SetBool("AimingMovement", true);
        currentAnimationEvent = () => StartCoroutine(ArrowInHandAnimation());
        
        if (isLocalPlayer)
        {
            UIController.instance.EnableTriangleCrosshair();
            CameraController.instance.EnableAimCamera();
            UIController.instance.ChangeTriangleAimSize(holdingTime / MAX_HOLDING_TIME);
        }
    }

    private IEnumerator ArrowInHandAnimation()
    {
        abilityAudioSource.clip = tenseSound;
        abilityAudioSource.Play();
        aiming = true;
        arrowInHand.SetActive(true);
        aimingParticles.Play();
        holdingTime = 0;
        while (aiming)
        {
            bowStringBone.position = rightHand.position;
            yield return null;
        }
    }

    private void ShootArrow(Vector3 target)
    {
        Arrow releasedArrow = quiver.GetArrowFromQuiver();
        float power = GetHoldingPowerPercent();
        releasedArrow.transform.position = arrowInHand.transform.position;
        releasedArrow.transform.LookAt(target);
        releasedArrow.SetPower(GetHoldingPowerPercent());
        releasedArrow.TriggerReleaseEffect();
        releasedArrow.rigidBody.velocity = releasedArrow.transform.forward * (20 + GetHoldingPowerPercent() * 30);
        if (isLocalPlayer)
            CameraController.instance.CreateShake(5, .1f);
    }

    protected override void StopHoldingAbility(Vector3 target)
    {
        if (abilityAudioSource.clip == tenseSound && abilityAudioSource.isPlaying)
            abilityAudioSource.Stop();
        if (aiming && holdingTime > MIN_HOLDING_TIME)
        {
            animator.SetTrigger("Shoot");
            ShootArrow(target);
            additionalyChestOffsetTime = 0.8f;
        }
        else
            additionalyChestOffsetTime = 0.05f;
        aimingParticles.Stop();
        bowStringBone.localPosition = bowBoneStartPosition;
        movement.IsAiming = false;
        arrowInHand.SetActive(false);
        aiming = false;
        animator.SetBool("Aiming", false);
        animator.SetBool("AimingMovement", false);

        AbilityEnd();
        if (isLocalPlayer)
        {
            CameraController.instance.DisableAimCamera();
            UIController.instance.EnableDefaultCrosshair();
        }
    }

    protected override void InterruptAbility()
    {
        if (isLocalPlayer)
            UIController.instance.EnableDefaultCrosshair();
        if (abilityAudioSource.clip == tenseSound && abilityAudioSource.isPlaying)
            abilityAudioSource.Stop();
        additionalyChestOffsetTime = 0;
        movement.IsAiming = false;
        bowStringBone.localPosition = bowBoneStartPosition;
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
