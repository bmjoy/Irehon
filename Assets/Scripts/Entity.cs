using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class Entity : NetworkBehaviour
{
    [SerializeField]
    protected float respawnTime;
    protected Vector3 spawnPosition;
    protected int health;
    [SerializeField]
    protected int maxHealth;
    [SerializeField]
    protected List<Collider> hitboxColliders = new List<Collider>();
    protected bool isAlive;
    protected EntityAnimatorController animator;

    protected virtual void Awake()
    {
        animator = GetComponent<EntityAnimatorController>();
    }

    protected virtual void Start()
    {
        spawnPosition = transform.position;
        SetDefaultState();
    }

    public List<Collider> GetHitBoxColliderList()
    {
        return hitboxColliders;
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
        animator?.PlayDeathAnimation();
        if (isServer)
            Invoke("Respawn", respawnTime);
    }

    public bool IsAlive() => isAlive;

    protected virtual void Respawn()
    {
        SetDefaultState();
        animator?.RespawnAnimation();
    }

    [Server]
    public virtual void Kill()
    {
        SetHealth(0);
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
    }

    [Server]
    public virtual void TakeDamage(int damage)
    {
        if (!isAlive)
            return;
        health -= damage;
        if (health <= 0)
        {
            Death();
            health = 0;
        }
        print(name + " got " + damage + " damage\nHealth = " + health);
    }
}
