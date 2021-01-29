using Mirror;
using UnityEngine;

public class Entity : NetworkBehaviour
{
    [SerializeField]
    protected int health;

    [Server]
    public virtual void TakeDamageOnServer(int damage)
    {
        health -= damage;
        if (health < 0)
            health = 0;
        print("got damage and health = " + health);
    }
}
