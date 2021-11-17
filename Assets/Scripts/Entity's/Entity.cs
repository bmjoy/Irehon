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

    [SerializeField, Tooltip("In seconds, 0 = will be destroyed after death")]
    protected float respawnTime;

    [SerializeField, Tooltip("Will be visible on healthbar")]
    protected new string name;

    protected Vector3 spawnPosition;

    [SyncVar(hook = "OnChangeHealth")]
    protected int health;

    [SerializeField, Tooltip("By default entity will spawn with this amount of health")]
    protected int maxHealth;

    protected HitConfirmEvent OnDoDamageEvent = new HitConfirmEvent();


    [SerializeField, Tooltip("Colliders ")]
    protected List<Collider> hitboxColliders = new List<Collider>();

    protected bool isAlive;
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

        OnDeathEvent.AddListener(() => Debug.Log($"<{name}> death event"));
        OnRespawnEvent.AddListener(() => Debug.Log($"<{name}> respawn event"));

        SetDefaultState();
    }

    protected void InitiateRespawn()
    {
        if (isServer)
        {
            Debug.Log("Started respawn");
            Invoke("Respawn", respawnTime);
        }
    }

    protected void OnChangeHealth(int oldValue, int newValue)
    {
        OnHealthChangeEvent.Invoke(maxHealth, health);
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


    private void SelfDestroy()
    {
        if (isServer)
            NetworkServer.Destroy(gameObject);
    }

    public bool IsAlive() => isAlive;

    protected virtual void Death()
    {
        Debug.Log($"Death {name}, isAlive = {isAlive}");

        if (!isAlive)
            return;

        isAlive = false;
        
        OnDeathEvent.Invoke();
        Debug.Log("Death event invoked");

        if (isServer)
            DeathClientRpc();
    }

    protected virtual void Respawn()
    {
        Debug.Log($"Respawn {name}, isAlive = {isAlive}");
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

    [TargetRpc]
    public void DoDamageEventTargetRpc(NetworkConnection con, int damage)
    {
        OnDoDamageEvent.Invoke(damage);
    }

    [TargetRpc]
    public void TakeDamageEventTargetRpc(int damage)
    {
        OnTakeDamageEvent.Invoke(damage);
    }

    [Server]
    public virtual void TakeDamage(DamageMessage damageMessage)
    {
        if (!isAlive)
            return;

        health -= damageMessage.damage;
        if (health <= 0)
        {
            Death();
            health = 0;
        }

        OnHealthChangeEvent?.Invoke(maxHealth, this.health);

        if (damageMessage.source != this)
            damageMessage.source?.DoDamageEventTargetRpc(damageMessage.source.connectionToClient, damageMessage.damage);
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
