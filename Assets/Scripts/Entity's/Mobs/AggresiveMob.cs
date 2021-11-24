using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AggresiveMob : Mob
{
    public Entity target;
    protected override void Start()
    {
        base.Start();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EntityBase"))
        {

        }
    }

    public override void TakeDamage(DamageMessage damageMessage)
    {
        if (!isAlive)
            return;
        target = damageMessage.source;
        base.TakeDamage(damageMessage);
    }
}

