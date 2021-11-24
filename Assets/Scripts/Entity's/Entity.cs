using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnHealthChangeEvent : UnityEvent<int, int> { }

public class HitConfirmEvent : UnityEvent<int> { }

public class OnTakeDamage : UnityEvent<int> { }

public struct DamageMessage
{
    public Entity source;
    public Entity target;
    public int damage;
}

public class Entity : NetworkBehaviour
{
    public List<Collider> HitboxColliders => hitboxColliders;
    public string NickName => name;
    public int Health => health;
    public Fraction fraction;
    public FractionBehaviourData FractionBehaviourData;

    [SerializeField, Tooltip("In seconds, 0 = will be destroyed after death")]
    protected float respawnTime;

    [SerializeField, Tooltip("Will be visible on healthbar")]
    protected new string name = "Entity";

    protected Vector3 spawnPosition;

    [SyncVar(hook = nameof(OnChangeHealth))]
    protected int health;

    [SerializeField, Tooltip("By default entity will spawn with this amount of health")]
    protected int maxHealth = 100;



    [SerializeField, Tooltip("Colliders ")]
    protected List<Collider> hitboxColliders = new List<Collider>();

    protected bool isAlive;

    public HitConfirmEvent OnDoDamageEvent { get; private set; } = new HitConfirmEvent();
    public OnTakeDamage OnTakeDamageEvent { get; private set; } = new OnTakeDamage();
    public OnHealthChangeEvent OnHealthChangeEvent { get; private set; } = new OnHealthChangeEvent();
    public UnityEvent OnDeathEvent { get; private set; } = new UnityEvent();
    public UnityEvent OnRespawnEvent { get; private set; } = new UnityEvent();

    protected virtual void Awake()
    {
        tag = "EntityBase";
    }

    private void Update()
    {
        if (transform.position.y < 0f)
            Death();
    }

    protected virtual void Start()
    {
        spawnPosition = transform.position;
        OnRespawnEvent.AddListener(SetDefaultState);

        if (respawnTime == 0)
            OnDeathEvent.AddListener(SelfDestroy);
        else
            OnDeathEvent.AddListener(InitiateRespawn);

        SetDefaultState();
    }

    protected void InitiateRespawn()
    {
        if (isServer)
        {
            Invoke("Respawn", respawnTime);
        }
    }

    protected void OnChangeHealth(int oldValue, int newValue)
    {
        OnHealthChangeEvent.Invoke(maxHealth, newValue);
    }

    public void SetName(string name)
    {
        this.name = name;
    }

    protected virtual void SetDefaultState()
    {
        isAlive = true;
        SetHealth(maxHealth);
        transform.position = spawnPosition;
    }


    private async void SelfDestroy()
    {
        if (isServer)
        {
            await System.Threading.Tasks.Task.Delay(500);
            NetworkServer.Destroy(gameObject);
        }
    }

    public bool IsAlive() => isAlive;

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
    protected virtual void DeathClientRpc() => Death();

    [ClientRpc]
    protected virtual void RespawnClientRpc() => Respawn();

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

        if (FractionBehaviourData != null)
        {
            if (FractionBehaviourData.Behaviours.ContainsKey(damageMessage.source.fraction) &&
               FractionBehaviourData.Behaviours[damageMessage.source.fraction] == FractionBehaviour.Friendly)
                return;
        }

        health -= damageMessage.damage;
        if (health <= 0)
        {
            Death();
            health = 0;
        }

        OnHealthChangeEvent?.Invoke(maxHealth, this.health);
        OnTakeDamageEvent.Invoke(damageMessage.damage);

        if (damageMessage.source != this)
            damageMessage.source?.OnDoDamageEvent.Invoke(damageMessage.damage);
    }

    [Server]
    public void DoDamage(Entity target, int damage)
    {
        DamageMessage damageMessage = new DamageMessage
        {
            damage = damage,
            source = this,
            target = target
        };
        target.TakeDamage(damageMessage);
    }
}
