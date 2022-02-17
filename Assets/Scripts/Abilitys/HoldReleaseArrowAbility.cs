using Irehon.Camera;
using System.Collections;
using UnityEngine;

namespace Irehon.Abilitys
{
    public class HoldReleaseArrowAbility : AbilityBase
    {
        private const float MAX_HOLDING_TIME = 1.7f;
        private const float MIN_HOLDING_TIME = 0.6f;

        private Bow bow;

        private Transform RightHand => this.boneLinks.RightHand;
        private Transform ShoulderForwardPoint => this.boneLinks.Shoulder;
        private GameObject Arrow => this.bow.Arrow;
        private AudioClip TenseSound => this.bow.TenseSound;
        private GameObject ArrowInHand => this.bow.ArrowInHand;
        private Transform BowStringBone => this.bow.BowStringBone;

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
            this.bow = this.weapon as Bow;
            this.bowBoneStartPosition = this.BowStringBone.localPosition;
            this.player = abilitySystem.player;
            this.animator = abilitySystem.animator;
            this.chestBone = this.animator.GetBoneTransform(HumanBodyBones.Chest);
            this.quiver = new Quiver(abilitySystem.AbilityPoolObject.transform, this.player, 5, this.Arrow, this.arrowDamage);
        }

        public void SetArrowDamage(int damage)
        {
            this.arrowDamage = damage;
        }

        private void LateUpdate()
        {
            if (this.aiming)
            {
                this.holdingTime += Time.deltaTime;
            }

            if (this.holdingTime > MAX_HOLDING_TIME)
            {
                this.holdingTime = MAX_HOLDING_TIME;
            }

            if (this.holdingTime > MIN_HOLDING_TIME && this.isLocalPlayer)
            {
                Crosshair.Instance.ChangeTriangleAimSize(this.GetHoldingPowerPercent());
            }

            if (this.aiming || this.additionalyChestOffsetTime > 0)
            {
                this.additionalyChestOffsetTime -= Time.deltaTime;
                this.chestBone.LookAt(this.ShoulderForwardPoint);
                this.chestBone.rotation *= Quaternion.Euler(this.aimingChestOffset);
            }
        }

        private float GetHoldingPowerPercent()
        {
            return (this.holdingTime - MIN_HOLDING_TIME) / (MAX_HOLDING_TIME - MIN_HOLDING_TIME);
        }

        protected override void Ability(Vector3 target)
        {
            if (abilitySystem.player.staminaPoints < Weapon.GetStaminaCost(weapon.GetType()))
            {
                return;
            }
            this.holdingTime = 0;
            this.additionalyChestOffsetTime = 5f;
            this.abilitySystem.BlockTrigger();
            this.animator.SetBool("Aiming", true);
            this.animator.SetBool("CastingMovement", true);
            this.currentAnimationEvent = () => this.StartCoroutine(this.ArrowInHandAnimation());

            if (this.isLocalPlayer)
            {
                Crosshair.Instance.EnableTriangleCrosshair();
                PlayerCameraSwitcher.Instance.EnableAimCamera();
                Crosshair.Instance.ChangeTriangleAimSize(this.holdingTime / MAX_HOLDING_TIME);
            }
        }

        private IEnumerator ArrowInHandAnimation()
        {
            if (!this.abilitySystem.IsAbilityCasting())
            {
                yield break;
            }

            this.abilitySystem.PlaySoundClip(this.TenseSound);
            this.aiming = true;
            this.ArrowInHand.SetActive(true);
            this.holdingTime = 0;
            while (this.aiming)
            {
                this.BowStringBone.position = this.RightHand.position;
                yield return null;
            }
        }

        private void ShootArrow(Vector3 target)
        {
            Arrow releasedArrow = this.quiver.GetArrowFromQuiver();

            releasedArrow.transform.position = this.ArrowInHand.transform.position;
            releasedArrow.transform.LookAt(target);
            releasedArrow.SetPower(this.GetHoldingPowerPercent());
            releasedArrow.TriggerReleaseEffect();
            releasedArrow.rigidBody.velocity = releasedArrow.transform.forward * (20 + this.GetHoldingPowerPercent() * 30);

            if (this.isLocalPlayer)
            {
                CameraShake.Instance.CreateShake(5, .1f);
            }
        }

        protected override void StopHoldingAbility(Vector3 target)
        {
            this.abilitySystem.StopPlayingClip(this.TenseSound);
            if (this.aiming && this.holdingTime > MIN_HOLDING_TIME)
            {
                this.animator.SetTrigger("Shoot");
                this.ShootArrow(target);
                abilitySystem.player.staminaPoints -= Weapon.GetStaminaCost(weapon.GetType());
                this.additionalyChestOffsetTime = 0.8f;
            }
            else
            {
                this.additionalyChestOffsetTime = 0.05f;
            }

            this.BowStringBone.localPosition = this.bowBoneStartPosition;
            this.abilitySystem.AllowTrigger();
            this.ArrowInHand.SetActive(false);
            this.aiming = false;
            this.animator.SetBool("Aiming", false);
            this.animator.SetBool("CastingMovement", false);

            this.AbilityEnd();
            if (this.isLocalPlayer)
            {
                PlayerCameraSwitcher.Instance.DisableAimCamera();
                Crosshair.Instance.EnableDefaultCrosshair();
            }
        }

        protected override void InterruptAbility()
        {
            if (this.isLocalPlayer)
            {
                PlayerCameraSwitcher.Instance.DisableAimCamera();
                Crosshair.Instance.EnableDefaultCrosshair();
            }
            this.ArrowInHand.SetActive(false);

            this.abilitySystem.StopPlayingClip(this.TenseSound);
            this.additionalyChestOffsetTime = 0;
            this.abilitySystem.AllowTrigger();
            this.BowStringBone.localPosition = this.bowBoneStartPosition;
            this.aiming = false;
            this.animator.ResetTrigger("Shoot");
            this.animator.SetBool("Aiming", false);
            this.animator.SetBool("CastingMovement", false);
        }

        private void OnDestroy()
        {
            Destroy(this.quiver.quiverTransform.gameObject);
            Destroy(this.quiver);
        }
    }
}