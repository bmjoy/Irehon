using Irehon.Entitys;
using UnityEngine;

public class TargetDummy : Entity
{
    private Animator animator;
    protected override void Start()
    {
        this.animator = this.GetComponent<Animator>();
        base.Start();

        SetMaxHealth(0, 0);

        HealthChanged += SetMaxHealth;
    }

    private void SetMaxHealth(int old, int current)
    {
        health = 10000;
    }
}
