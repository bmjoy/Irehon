using Irehon.Entitys;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon.Abilitys
{
    public class Arrow : MonoBehaviour
    {
        protected const float TIME_TO_DESPAWN = 4f;
        protected int hitDamage;
        [SerializeField]
        protected ParticleSystem releaseEffect;
        public Rigidbody rigidBody;
        [SerializeField]
        protected AudioSource releaseSound;
        protected float power;
        protected bool flying = true;
        protected float time = 0f;
        [SerializeField]
        protected SniperArrowParticle particle;
        protected Player arrowOwner;
        protected Quiver quiver;
        protected List<Collider> selfColliders;

        public void ResetArrow()
        {
            this.time = 0;
            this.flying = true;
            this.rigidBody.useGravity = true;
        }

        public void TriggerReleaseEffect()
        {
            if (this.releaseSound != null)
            {
                this.releaseSound.Play();
            }

            if (this.releaseEffect != null)
            {
                this.releaseEffect.Play();
            }
        }

        public void SetDamage(int damage)
        {
            this.hitDamage = damage;
        }

        protected int GetDamage()
        {
            return Convert.ToInt32(this.hitDamage * .3f + this.hitDamage * this.power);
        }

        protected void Update()
        {
            this.time += Time.deltaTime;
            if (this.flying && this.rigidBody.velocity != Vector3.zero)
            {
                this.transform.rotation = Quaternion.LookRotation(this.rigidBody.velocity);
            }

            if (this.time > TIME_TO_DESPAWN)
            {
                this.quiver.ReturnArrowInQuiver(this);
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("EntityBase") || other.CompareTag("Ability") || !this.flying)
            {
                return;
            }

            if (this.selfColliders.Contains(other))
            {
                return;
            }

            EntityCollider entityCollider = other.GetComponent<EntityCollider>();
            if (entityCollider != null && !entityCollider.GetParentEntityComponent().isAlive)
            {
                return;
            }

            this.flying = false;
            this.rigidBody.useGravity = false;
            this.rigidBody.velocity = Vector3.zero;

            this.HittedColliderProcess(other);
            if (other.CompareTag("Entity"))
            {
                this.quiver.ReturnArrowInQuiver(this);
            }
        }

        public void HittedColliderProcess(Collider collider)
        {
            if (collider.CompareTag("Entity"))
            {
                EntityCollider entityCollider = collider.GetComponent<EntityCollider>();
            }
        }

        public void SetParent(Player arrowOwner, List<Collider> selfColliders, Quiver quiver)
        {
            this.arrowOwner = arrowOwner;
            this.selfColliders = selfColliders;
            this.quiver = quiver;
        }

        public void SetPower(float power)
        {
            this.power = power;
            if (this.particle != null)
            {
                this.particle.SetWaveSize(power);
            }
        }
    }
}