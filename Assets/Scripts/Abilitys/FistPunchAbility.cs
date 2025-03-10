﻿using Irehon.Camera;
using Irehon.Entitys;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon.Abilitys
{
    public class FistPunchAbility : AbilityBase
    {
        [SerializeField]
        private GameObject handFistColliderPrefab;

        [SerializeField]
        private AudioClip onAttackStartSound;
        [SerializeField]
        private AudioClip onImpactSound;

        private MeleeWeaponCollider leftHandCollider;

        public override void Setup(AbilitySystem abilitySystem)
        {
            base.Setup(abilitySystem);
            this.leftHandCollider = Instantiate(this.handFistColliderPrefab, abilitySystem.playerBonesLinks.LeftHand).GetComponent<MeleeWeaponCollider>();

            this.leftHandCollider.transform.localPosition = Vector3.zero;

            this.leftHandCollider.Intialize(abilitySystem.player.HitboxColliders);

            this.currentAnimationEvent = this.DamageEntitiesInArea;
        }
        protected override void Ability(Vector3 target)
        {
            if (abilitySystem.player.staminaPoints < Weapon.GetStaminaCost(weapon.GetType()))
            {
                return;
            }
            abilitySystem.player.staminaPoints -= Weapon.GetStaminaCost(weapon.GetType());
            this.abilitySystem.animator.SetTrigger("Skill1");
            this.leftHandCollider.StartCollectColliders();
            this.AbilityStart();
        }

        public override void AbilitySoundEvent()
        {
            this.abilitySystem.PlaySoundClip(this.onAttackStartSound);
        }
        private void DamageEntitiesInArea()
        {
            if (this.isLocalPlayer)
            {
                CameraShake.Instance.CreateShake(1, .3f);
            }

            if (this.leftHandCollider.GetCollectedInZoneEntities().Count != 0)
            {
                this.abilitySystem.PlaySoundClip(this.onImpactSound);
            }
        }

        private int GetDamage()
        {
            return 100;
        }

        public void DestroyColliders()
        {
            Destroy(this.leftHandCollider.gameObject);
        }

        protected override void InterruptAbility()
        {
            this.abilitySystem.animator.ResetTrigger("Skill1");
            this.AbilityEnd();
        }

        protected override void StopHoldingAbility(Vector3 target)
        {
        }
    }
}