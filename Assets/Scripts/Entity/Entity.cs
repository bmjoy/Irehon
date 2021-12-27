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
            Respawned += this.SetDefaultState;

            if (this.respawnTime == 0)
            {
                Dead += this.SelfDestroy;
            }
            else
            {
                Dead += this.InitiateRespawn;
            }

            if (this.isServer)
            {
                DidDamage += this.OnDoDamageRpc;
                GotDamage += this.OnTakeDamageRpc;
                takeDamageProcessQuerry.Add(NonAliveDamageBlocker);
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
            if (isServer)
                this.isAlive = true;
            IsAliveValueChanged?.Invoke(isAlive);
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
            IsAliveValueChanged?.Invoke(isAlive);

            Dead?.Invoke();

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
            IsAliveValueChanged?.Invoke(isAlive);

            Respawned?.Invoke();

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
            DidDamage?.Invoke(damage);
        }

        [ClientRpc]
        protected void OnTakeDamageRpc(int damage)
        {
            GotDamage?.Invoke(damage);
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

            HealthChanged?.Invoke(this.maxHealth, this.health);
        }

        private void NonAliveDamageBlocker(ref DamageMessage damageMessage)
        {
            if (!isAlive)
                damageMessage.damage = 0;
        }

        [Server]
        public virtual void TakeDamage(DamageMessage damageMessage)
        {
            foreach (var process in takeDamageProcessQuerry)
            {
                if (damageMessage.damage > 0)
                    process(ref damageMessage);
            }

            if (damageMessage.damage <= 0)
                return;

            ServerManager.Log($"{damageMessage.source} did {damageMessage.damage} damage to {damageMessage.target.name}");

            this.health -= damageMessage.damage;

            if (this.health <= 0)
            {
                KilledByEntity?.Invoke(damageMessage.source);
                damageMessage.source.OnKillEvent?.Invoke(this);
                this.Death();
                this.health = 0;
            }

            HealthChanged?.Invoke(this.maxHealth, this.health);
            GotDamage?.Invoke(damageMessage.damage);

            if (damageMessage.source != this)
            {
                damageMessage.source?.DidDamage?.Invoke(damageMessage.damage);
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

            foreach (var process in doDamageProcessQuerry)
            {
                if (damageMessage.damage > 0)
                    process(ref damageMessage);
            }
            
            if (damageMessage.damage <= 0)
                return false;

            ServerManager.Log($"{damageMessage.source} sended Damage Message with {damageMessage.damage} damage to {damageMessage.target.name}");

            target.TakeDamage(damageMessage);
            return true;
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