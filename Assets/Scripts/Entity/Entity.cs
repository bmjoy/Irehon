using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon.Entitys
{
    public class Entity : NetworkBehaviour
    {
        public delegate void EntityVoidEventHandler();
        public delegate void EntityEventHandler(object sender);
        public delegate void EntityIntEventHandler(int value);
        public delegate void EntityStateIntEventHandler(int old, int current);
        public string NickName => this.name;
        public int Health => this.health;
        public FractionBehaviourData FractionBehaviourData;
        [SyncVar]
        public Fraction fraction;

        public float respawnTime;

        public new string name = "Entity";

        public Vector3 startPosition { get; protected set; }

        [SyncVar(hook = nameof(OnChangeHealth))]
        protected int health;

        public int maxHealth = 100;

        public List<Collider> HitboxColliders = new List<Collider>();
        [SyncVar]
        public bool isAlive;
        /// <summary>
        /// Invoked only on server, pass entity that been killed by this entity
        /// </summary>
        public event EntityEventHandler OnKillEvent;
        /// <summary>
        /// Invoked only on server, pass entity that killed this entity
        /// </summary>
        public event EntityEventHandler OnGetKilledEvent;
        public event EntityIntEventHandler OnDoDamageEvent;
        public event EntityIntEventHandler OnTakeDamageEvent;
        public event EntityStateIntEventHandler OnHealthChangeEvent;
        public event EntityVoidEventHandler OnDeathEvent;
        public event EntityVoidEventHandler OnRespawnEvent;
        public event EntityVoidEventHandler OnPlayerLookingEvent;

        protected virtual void Awake()
        {
            this.tag = "EntityBase";
            this.gameObject.layer = 14;
        }

        protected virtual void Start()
        {
            this.startPosition = this.transform.position;
            OnRespawnEvent += this.SetDefaultState;

            if (this.respawnTime == 0)
            {
                OnDeathEvent += this.SelfDestroy;
            }
            else
            {
                OnDeathEvent += this.InitiateRespawn;
            }

            if (this.isServer)
            {
                OnDoDamageEvent += this.OnDoDamageRpc;
                OnTakeDamageEvent += this.OnTakeDamageRpc;
            }
            this.SetDefaultState();
        }
        private void Update()
        {
            if (this.transform.position.y < 0f && this.isAlive)
            {
                this.Death();
            }
        }
        protected void InitiateRespawn()
        {
            if (this.isServer)
            {
                this.Invoke("Respawn", this.respawnTime);
            }
        }

        protected void OnChangeHealth(int oldValue, int newValue)
        {
            OnHealthChangeEvent?.Invoke(this.maxHealth, newValue);
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public virtual void SetDefaultState()
        {
            this.isAlive = true;
            this.SetHealth(this.maxHealth);
            this.transform.position = this.startPosition;
        }


        private async void SelfDestroy()
        {
            if (this.isServer)
            {
                await System.Threading.Tasks.Task.Delay(500);
                NetworkServer.Destroy(this.gameObject);
            }
        }

        protected virtual void Death()
        {
            if (!this.isAlive)
            {
                return;
            }

            this.isAlive = false;

            OnDeathEvent?.Invoke();

            if (this.isServer)
            {
                this.DeathClientRpc();
            }
        }

        protected virtual void Respawn()
        {
            if (this.isAlive)
            {
                return;
            }

            this.isAlive = true;

            OnRespawnEvent?.Invoke();

            if (this.isServer)
            {
                this.RespawnClientRpc();
            }
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
            OnDoDamageEvent?.Invoke(damage);
        }

        [ClientRpc]
        protected void OnTakeDamageRpc(int damage)
        {
            OnTakeDamageEvent?.Invoke(damage);
        }

        [Server]
        protected virtual void SetHealth(int health)
        {
            this.health = health;

            if (this.health > this.maxHealth)
            {
                this.health = this.maxHealth;
            }

            if (this.health <= 0)
            {
                this.health = 0;
                this.Death();
            }

            OnHealthChangeEvent?.Invoke(this.maxHealth, this.health);
        }

        [Server]
        public virtual void TakeDamage(DamageMessage damageMessage)
        {
            if (!this.isAlive)
            {
                return;
            }

            this.health -= damageMessage.damage;

            if (this.health <= 0)
            {
                OnGetKilledEvent?.Invoke(damageMessage.source);
                damageMessage.source.OnKillEvent?.Invoke(this);
                this.Death();
                this.health = 0;
            }

            OnHealthChangeEvent?.Invoke(this.maxHealth, this.health);
            OnTakeDamageEvent?.Invoke(damageMessage.damage);

            if (damageMessage.source != this)
            {
                damageMessage.source?.OnDoDamageEvent?.Invoke(damageMessage.damage);
            }
        }

        [Server]
        public bool DoDamage(Entity target, int damage)
        {
            if (this.FractionBehaviourData != null && this.FractionBehaviourData.Behaviours.ContainsKey(target.fraction) &&
                   this.FractionBehaviourData.Behaviours[target.fraction] == FractionBehaviour.Friendly)
            {
                return false;
            }

            DamageMessage damageMessage = new DamageMessage
            {
                damage = damage,
                source = this,
                target = target
            };
            target.TakeDamage(damageMessage);
            return true;
        }

        public void InvokePlayerLookingEvent()
        {
            OnPlayerLookingEvent?.Invoke();
        }
    }
}