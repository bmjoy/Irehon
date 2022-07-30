using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon.Entitys
{
    public class Entity : NetworkBehaviour
    {
        public delegate void EntityDamageQuerryProcess(ref DamageMessage message);
        public delegate void EntityVoidEventHandler();
        public delegate void EntityAliveEventHandler(bool isAlive);
        public delegate void EntityEventHandler(Entity sender);
        public delegate void EntityIntEventHandler(int value);
        public delegate void EntityStateIntEventHandler(int old, int current);
        public string NickName => this.name;
        public int Health => this.health;

        [SyncVar(hook = nameof(IsAliveHook))]
        public bool isAlive;
        [SyncVar]
        public Fraction fraction;

        public FractionBehaviourData FractionBehaviourData;
        public float respawnTime;
        public new string name = "Entity";

        public Vector3 startPosition { get; protected set; }

        [SyncVar(hook = nameof(ChangeHealthHook))]
        protected int health;

        public int maxHealth = 100;

        public List<Collider> HitboxColliders = new List<Collider>();
        public List<EntityDamageQuerryProcess> doDamageProcessQuerry = new List<EntityDamageQuerryProcess>();
        public List<EntityDamageQuerryProcess> takeDamageProcessQuerry = new List<EntityDamageQuerryProcess>();
        /// <summary>
        /// Invoked only on server, pass entity that been killed by this entity
        /// </summary>
        public event EntityEventHandler OnKillEvent;
        /// <summary>
        /// Invoked only on server, pass entity that killed this entity
        /// </summary>
        public event EntityAliveEventHandler IsAliveValueChanged;
        public event EntityEventHandler KilledByEntity;
        public event EntityIntEventHandler DidDamage;
        public event EntityIntEventHandler GotDamage;
        public event EntityStateIntEventHandler HealthChanged;
        public event EntityVoidEventHandler Dead;
        public event EntityVoidEventHandler Respawned;
        public event EntityVoidEventHandler PlayerLooked;

        protected virtual void Awake()
        {
            this.tag = "EntityBase";
            this.gameObject.layer = 14;
        }

        protected virtual void Start()
        {
            this.startPosition = this.transform.position;
            startPosition = new Vector3(startPosition.x, startPosition.y + 0.5f, startPosition.z);
            Respawned += this.SetDefaultState;

            this.SetDefaultState();
        }

        protected void ChangeHealthHook(int oldValue, int newValue)
        {
            HealthChanged?.Invoke(this.maxHealth, newValue);
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public virtual void SetDefaultState()
        {
            IsAliveValueChanged?.Invoke(isAlive);
            this.transform.position = this.startPosition;
        }

        protected virtual void Death()
        {
            if (!this.isAlive)
            {
                return;
            }

            this.isAlive = false;
            IsAliveValueChanged?.Invoke(isAlive);

            Dead?.Invoke();
        }

        protected virtual void Respawn()
        {
            if (this.isAlive)
            {
                return;
            }

            this.isAlive = true;
            IsAliveValueChanged?.Invoke(isAlive);

            Respawned?.Invoke();
        }

        [ClientRpc]
        private void DeathClientRpc()
        {
            this.Death();
        }

        [ClientRpc]
        private void RespawnClientRpc()
        {
            this.Respawn();
        }

        [ClientRpc]
        protected void OnDoDamageRpc(int damage)
        {
            DidDamage?.Invoke(damage);
        }

        [ClientRpc]
        protected void OnTakeDamageRpc(int damage)
        {
            GotDamage?.Invoke(damage);
        }

        public void InvokePlayerLookingEvent()
        {
            PlayerLooked?.Invoke();
        }

        protected virtual void IsAliveHook(bool oldValue, bool newValue)
        {
            IsAliveValueChanged?.Invoke(newValue);
        }
    }
}