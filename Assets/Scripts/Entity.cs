using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class Entity : NetworkBehaviour
{
    [SerializeField]
    protected int health;
    [SerializeField]
    protected int maxHealth;
    [SerializeField]
    private List<Collider> hitboxColliders = new List<Collider>();

    public List<Collider> GetHitBoxColliderList()
    {
        return hitboxColliders;
    }

    [Server]
    public virtual void TakeDamageOnServer(int damage)
    {
        health -= damage;
        if (health < 0)
            health = 0;
        print(name + " got " + damage + " damage\nHealth = " + health);
    }
}
