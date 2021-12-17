using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public struct DamageMessage
{
    public Entity source;
    public Entity target;
    public int damage;
}

public class Entity : NetworkBehaviour
{
    public delegate void EntityVoidEventHandler();
    public delegate void EntityEventHandler(object sender);
    public delegate void EntityIntEventHandler(int value);
    public delegate void EntityStateIntEventHandler(int old, int current);
    public string NickName => name;
    public int Health => health;
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
        tag = "EntityBase";
        gameObject.layer = 14;
    }

    protected virtual void Start()
    {
        startPosition = transform.position;
        OnRespawnEvent += SetDefaultState;

        if (respawnTime == 0)
            OnDeathEvent += SelfDestroy;
        else
            OnDeathEvent += InitiateRespawn;

        if (isServer)
        {
            OnDoDamageEvent += OnDoDamageRpc;
            OnTakeDamageEvent += OnTakeDamageRpc;
        }
        SetDefaultState();
    }
    private void Update()
    {
        if (transform.position.y < 0f && isAlive)
            Death();
    }
    protected void InitiateRespawn()
    {
        if (isServer)
            Invoke("Respawn", respawnTime);
    }

    protected void OnChangeHealth(int oldValue, int newValue)
    {
        OnHealthChangeEvent?.Invoke(maxHealth, newValue);
    }

    public void SetName(string name)
    {
        this.name = name;
    }

    public virtual void SetDefaultState()
    {
        isAlive = true;
        SetHealth(maxHealth);
        transform.position = startPosition;
    }


    private async void SelfDestroy()
    {
        if (isServer)
        {
            await System.Threading.Tasks.Task.Delay(500);
            NetworkServer.Destroy(gameObject);
        }
    }

    protected virtual void Death()
    {
        if (!isAlive)
            return;

        isAlive = false;
        
        OnDeathEvent?.Invoke();

        if (isServer)
            DeathClientRpc();
    }

    protected virtual void Respawn()
    {
        if (isAlive)
            return;

        isAlive = true;

        OnRespawnEvent?.Invoke();

        if (isServer)
            RespawnClientRpc();
    }

    [ClientRpc]
    private void DeathClientRpc() => Death();

    [ClientRpc]
    private void RespawnClientRpc() => Respawn();
    [ClientRpc]
    protected void OnDoDamageRpc(int damage) => OnDoDamageEvent?.Invoke(damage);
    [ClientRpc]
    protected void OnTakeDamageRpc(int damage) => OnTakeDamageEvent?.Invoke(damage);

    [Server]
    protected virtual void SetHealth(int health)
    {
        this.health = health;

        if (this.health > maxHealth)
            this.health = maxHealth;

        if (this.health <= 0)
        {
            this.health = 0;
            Death();
        }

        OnHealthChangeEvent?.Invoke(maxHealth, this.health);
    }

    [Server]
    public virtual void TakeDamage(DamageMessage damageMessage)
    {
        if (!isAlive)
            return;

        health -= damageMessage.damage;

        if (health <= 0)
        {
            OnGetKilledEvent?.Invoke(damageMessage.source);
            damageMessage.source.OnKillEvent?.Invoke(this);
            Death();
            health = 0;
        }

        OnHealthChangeEvent?.Invoke(maxHealth, this.health);
        OnTakeDamageEvent?.Invoke(damageMessage.damage);

        if (damageMessage.source != this)
        {
            damageMessage.source?.OnDoDamageEvent?.Invoke(damageMessage.damage);
        }
    }

    [Server]
    public bool DoDamage(Entity target, int damage)
    {
        if (FractionBehaviourData != null && FractionBehaviourData.Behaviours.ContainsKey(target.fraction) &&
               FractionBehaviourData.Behaviours[target.fraction] == FractionBehaviour.Friendly)
            return false;

        DamageMessage damageMessage = new DamageMessage
        {
            damage = damage,
            source = this,
            target = target
        };
        target.TakeDamage(damageMessage);
        return true;
    }

    public void InvokePlayerLookingEvent() => OnPlayerLookingEvent?.Invoke();
}
