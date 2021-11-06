using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthChangedEvent : UnityEvent<int, int> {}

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
    public string NickName { get => name; }
    public int Health { get => health; }

    [SerializeField]
    protected float respawnTime;

    [SerializeField]
    protected new string name;

    protected Vector3 spawnPosition;

    [SyncVar(hook = "OnChangeHealth")]
    protected int health;

    [SerializeField]
    protected int maxHealth;

    protected HitConfirmEvent HitConfirmEvent = new HitConfirmEvent();

    [SerializeField]
    protected List<Collider> hitboxColliders = new List<Collider>();

    protected bool isAlive;
    public OnTakeDamage OnTakeDamageEvent = new OnTakeDamage();
    public HealthChangedEvent OnHealthChanged;

    protected virtual void Awake()
    {
        if (OnHealthChanged == null)
            OnHealthChanged = new HealthChangedEvent();
    }

    private void Update()
    {
        if (transform.position.y < 0f)
            Death();
    }

    protected virtual void Start()
    {
        spawnPosition = transform.position;
        SetDefaultState();
    }

    protected void OnChangeHealth(int oldValue, int newValue)
    {
        OnHealthChanged.Invoke(maxHealth, health);
    }

    public List<Collider> GetHitBoxColliderList()
    {
        return hitboxColliders;
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

    protected virtual void Death()
    {
        isAlive = false;
        if (isServer)
        {
            if (respawnTime > 0)
                Invoke("Respawn", respawnTime);
            else
                StartCoroutine(SelfDestroy());
        }
    }

    private IEnumerator SelfDestroy()
    {
        for (int i = 0; i < 20; i++)
        {
            i++;
            transform.Translate(Vector3.down * 0.15f);
            yield return new WaitForSeconds(0.1f);
        }
        NetworkServer.Destroy(gameObject);
    }

    public bool IsAlive() => isAlive;

    protected virtual void Respawn()
    {
        SetDefaultState();
    }

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

        OnHealthChanged?.Invoke(maxHealth, this.health);
    }

    [TargetRpc]
    public void EntityHitConfirm(NetworkConnection con, int damage)
    {
        HitConfirmEvent.Invoke(damage);
    }

    [TargetRpc]
    public void TakeDamageEvent(int damage)
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
        OnHealthChanged?.Invoke(maxHealth, this.health);
        if (damageMessage.source != this)
            damageMessage.source?.EntityHitConfirm(damageMessage.source.connectionToClient, damageMessage.damage);
    }
}
