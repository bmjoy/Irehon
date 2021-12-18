using UnityEngine;

namespace Irehon.Abilitys
{
    public abstract class AbilityBase : MonoBehaviour
    {
        public KeyCode TriggerKey => this.triggerOnKey;

        protected bool isLocalPlayer => this.abilitySystem.isLocalPlayer;
        protected bool isServer => this.abilitySystem.isServer;

        protected Coroutine cooldownCoroutine;
        protected AbilitySystem abilitySystem;
        protected PlayerBonesLinks boneLinks;
        protected Weapon weapon;

        protected CurrentAnimationEvent currentAnimationEvent;
        protected delegate void CurrentAnimationEvent();

        [SerializeField]
        protected float cooldownTime;
        [SerializeField]
        protected KeyCode triggerOnKey;

        public virtual void Setup(AbilitySystem abilitySystem)
        {
            this.abilitySystem = abilitySystem;
            this.boneLinks = abilitySystem.playerBonesLinks;
            this.weapon = this.GetComponent<Weapon>();
        }

        protected abstract void StopHoldingAbility(Vector3 target);
        protected abstract void Ability(Vector3 target);
        protected abstract void InterruptAbility();

        public void Interrupt()
        {
            this.InterruptAbility();
        }

        public void AnimationEvent()
        {
            if (this.currentAnimationEvent != null)
            {
                this.currentAnimationEvent();
            }
        }

        public void TriggerKeyUp(Vector3 target)
        {
            this.StopHoldingAbility(target);
        }

        public bool TriggerKeyDown(Vector3 target)
        {
            this.Ability(target);
            return true;
        }

        public virtual void AbilitySoundEvent()
        {

        }

        public virtual void SubEvent()
        {

        }

        protected void AbilityStart()
        {
            this.abilitySystem.BlockTrigger();
        }

        protected void AbilityEnd()
        {
            this.abilitySystem.AllowTrigger();
        }
    }
}