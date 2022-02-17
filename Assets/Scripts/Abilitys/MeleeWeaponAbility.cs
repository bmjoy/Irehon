using UnityEngine;
using UnityEngine.Events;
using Irehon.Entitys;
using Irehon.Camera;

namespace Irehon.Abilitys
{
    [RequireComponent(typeof(MeleeWeaponCollider))]
    public class MeleeWeaponAbility : AbilityBase
    {
        public UnityEvent OnClientAttackStart;

        private MeleeWeaponCollider meleeCollider;
        [SerializeField]
        private AudioClip onAttackStartSound;
        [SerializeField]
        private AudioClip onImpactSound;

        private int damage;

        public override void Setup(AbilitySystem abilitySystem)
        {
            base.Setup(abilitySystem);

            this.meleeCollider = this.GetComponent<MeleeWeaponCollider>();
            this.meleeCollider.Intialize(abilitySystem.player.HitboxColliders);

            this.currentAnimationEvent = this.DamageEntitiesInArea;

            abilitySystem.player.DidDamage += this.ImpactSound;

            this.meleeCollider.OnNewCollectedEntityEvent.AddListener(this.DamageEntity);
        }
        protected override void Ability(Vector3 target)
        {
            if (abilitySystem.player.staminaPoints < Weapon.GetStaminaCost(weapon.GetType()))
            {
                return;
            }
            abilitySystem.player.staminaPoints -= Weapon.GetStaminaCost(weapon.GetType());
            this.abilitySystem.animator.SetTrigger("Skill1");
            this.AbilityStart();
            if (this.abilitySystem.isClient)
            {
                this.OnClientAttackStart.Invoke();
            }
        }

        public override void SubEvent()
        {
            this.meleeCollider.StartCollectColliders();
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

            if (!this.isServer)
            {
                return;
            }

            this.meleeCollider.StopCollectColliders();

            this.abilitySystem.animator.ResetTrigger("Skill1");
            this.AbilityEnd();
        }

        private void DamageEntity(Entity entity, EntityCollider entityCollider)
        {
            this.abilitySystem.player.DoDamage(entity, Mathf.RoundToInt(this.GetDamage() * entityCollider.damageMultiplier));
        }

        private void ImpactSound(int damage)
        {
            if (damage != 0)
            {
                this.abilitySystem.PlaySoundClip(this.onImpactSound);
            }
        }

        private int GetDamage()
        {
            return this.damage;
        }

        public int SetDamage(int damage)
        {
            return this.damage = damage;
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