using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IntUpdateEvent : UnityEvent<int, int> { }

public class IntEvent : UnityEvent<int> { }
public class EntityEvent : UnityEvent<Entity> { }

public struct DamageMessage
{
    public Entity source;
    public Entity target;
    public int damage;
}

public class Entity : NetworkBehaviour
{
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
    public EntityEvent OnKillEvent { get; private set; } = new EntityEvent();
    /// <summary>
    /// Invoked only on server, pass entity that killed this entity
    /// </summary>
    public EntityEvent OnGetKilledEvent { get; private set; } = new EntityEvent();
    public IntEvent OnDoDamageEvent { get; private set; } = new IntEvent();
    public IntEvent OnTakeDamageEvent { get; private set; } = new IntEvent();
    public IntUpdateEvent OnHealthChangeEvent { get; private set; } = new IntUpdateEvent();
    public UnityEvent OnDeathEvent { get; private set; } = new UnityEvent();
    public UnityEvent OnRespawnEvent { get; private set; } = new UnityEvent();
    public UnityEvent OnPlayerLookingEvent { get; private set; } = new UnityEvent();

    protected virtual void Awake()
    {
        tag = "EntityBase";
        gameObject.layer = 14;
    }

    protected virtual void Start()
    {
        startPosition = transform.position;
        OnRespawnEvent.AddListener(SetDefaultState);

        if (respawnTime == 0)
            OnDeathEvent.AddListener(SelfDestroy);
        else
            OnDeathEvent.AddListener(InitiateRespawn);

        if (isServer)
        {
            OnDoDamageEvent.AddListener(OnDoDamageRpc);
            OnTakeDamageEvent.AddListener(OnTakeDamageRpc);
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
        OnHealthChangeEvent.Invoke(maxHealth, newValue);
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
        
        OnDeathEvent.Invoke();

        if (isServer)
            DeathClientRpc();
    }

    protected virtual void Respawn()
    {
        if (isAlive)
            return;

        isAlive = true;

        OnRespawnEvent.Invoke();

        if (isServer)
            RespawnClientRpc();
    }

    [ClientRpc]
    private void DeathClientRpc() => Death();

    [ClientRpc]
    private void RespawnClientRpc() => Respawn();
    [ClientRpc]
    protected void OnDoDamageRpc(int damage) => OnDoDamageEvent.Invoke(damage);
    [ClientRpc]
    protected void OnTakeDamageRpc(int damage) => OnTakeDamageEvent.Invoke(damage);

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
            OnGetKilledEvent.Invoke(damageMessage.source);
            damageMessage.source.OnKillEvent.Invoke(this);
            Death();
            health = 0;
        }

        OnHealthChangeEvent?.Invoke(maxHealth, this.health);
        OnTakeDamageEvent.Invoke(damageMessage.damage);

        if (damageMessage.source != this)
        {
            damageMessage.source?.OnDoDamageEvent.Invoke(damageMessage.damage);
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
}
