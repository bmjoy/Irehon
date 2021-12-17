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
        Debug.LogError("Ne nastroen!!");
        //animator.SetTrigger("GotDamage");
        //if (damageMessage.source != null)
        //    damageMessage.source.OnDoDamageEvent.Invoke(damageMessage.damage);
    }
}
