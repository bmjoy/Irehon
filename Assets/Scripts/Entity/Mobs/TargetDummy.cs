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
}
