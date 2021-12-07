using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldReleaseArrowAbility : AbilityBase
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

    private Vector3 aimingChestOffset = new Vector3(331.6f, 260.2f, -150.2f);

    private Vector3 bowBoneStartPosition;
    [SerializeField]
    private Transform chestBone;
    private Animator animator;
    private Quiver quiver;
    private Player player;

    private float holdingTime;
    private float additionalyChestOffsetTime;
    private bool aiming;

    private int arrowDamage;

    public override void Setup(AbilitySystem abilitySystem)
    {
        base.Setup(abilitySystem);
        bow = weapon as Bow;
        bowBoneStartPosition = BowStringBone.localPosition;
        player = abilitySystem.player;
        animator = abilitySystem.animator;
        chestBone = animator.GetBoneTransform(HumanBodyBones.Chest);
        quiver = new Quiver(abilitySystem.AbilityPoolObject.transform, player, 5, Arrow, arrowDamage);
    }

    public void SetArrowDamage(int damage) => arrowDamage = damage;

    private void LateUpdate()
    {
        if (aiming)
            holdingTime += Time.deltaTime;
        if (holdingTime > MAX_HOLDING_TIME)
            holdingTime = MAX_HOLDING_TIME;
        if (holdingTime > MIN_HOLDING_TIME && isLocalPlayer)
            UIController.i.ChangeTriangleAimSize(GetHoldingPowerPercent());
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
        animator.SetBool("CastingMovement", true);
        currentAnimationEvent = () => StartCoroutine(ArrowInHandAnimation());

        if (isLocalPlayer)
        {
            UIController.i.EnableTriangleCrosshair();
            CameraController.EnableAimCamera();
            UIController.i.ChangeTriangleAimSize(holdingTime / MAX_HOLDING_TIME);
        }
    }

    private IEnumerator ArrowInHandAnimation()
    {
        abilitySystem.PlaySoundClip(TenseSound);
        aiming = true;
        ArrowInHand.SetActive(true);
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
        abilitySystem.StopPlayingClip(TenseSound);
        if (aiming && holdingTime > MIN_HOLDING_TIME)
        {
            animator.SetTrigger("Shoot");
            ShootArrow(target);
            additionalyChestOffsetTime = 0.8f;
        }
        else
            additionalyChestOffsetTime = 0.05f;

        BowStringBone.localPosition = bowBoneStartPosition;
        abilitySystem.AllowTrigger();
        ArrowInHand.SetActive(false);
        aiming = false;
        animator.SetBool("Aiming", false);
        animator.SetBool("CastingMovement", false);

        AbilityEnd();
        if (isLocalPlayer)
        {
            CameraController.DisableAimCamera();
            UIController.i.EnableDefaultCrosshair();
        }
    }

    protected override void InterruptAbility()
    {
        if (isLocalPlayer)
        {
            CameraController.DisableAimCamera();
            UIController.i.EnableDefaultCrosshair();
        }
        ArrowInHand.SetActive(false);

        abilitySystem.StopPlayingClip(TenseSound);
        additionalyChestOffsetTime = 0;
        abilitySystem.AllowTrigger();
        BowStringBone.localPosition = bowBoneStartPosition;
        aiming = false;
        animator.ResetTrigger("Shoot");
        animator.SetBool("Aiming", false);
        animator.SetBool("CastingMovement", false);
    }

    private void OnDestroy()
    {
        Destroy(quiver.quiverTransform.gameObject);
        Destroy(quiver);
    }
}
