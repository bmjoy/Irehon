using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TargetDummy : Entity
{
    private Animator animator;
    protected override void Start()
    {
        animator = GetComponent<Animator>();
        base.Start();
    }
    public override void TakeDamage(DamageMessage damageMessage)
    {
        animator.SetTrigger("GotDamage");
        damageMessage.source?.OnDoDamageEvent.Invoke(damageMessage.damage);
    }
}
