using Irehon.Entitys;
using UnityEngine;

public class TargetDummy : Entity
{
    private Animator animator;
    protected override void Start()
    {
        this.animator = this.GetComponent<Animator>();
        base.Start();

        takeDamageProcessQuerry.Add(DamageMessageNuller);
    }

    private void DamageMessageNuller(ref DamageMessage message)
    {
        message.damage = 0;
    }

    public override void TakeDamage(DamageMessage damageMessage)
    {
        Debug.LogError("Ne nastroen!!");
        //animator.SetTrigger("GotDamage");
        //if (damageMessage.source != null)
        //    damageMessage.source.OnDoDamageEvent.Invoke(damageMessage.damage);
    }
}
