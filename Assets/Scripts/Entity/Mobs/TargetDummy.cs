using Irehon.Entitys;
using UnityEngine;

public class TargetDummy : Entity
{
    private Animator animator;
    protected override void Start()
    {
        this.animator = this.GetComponent<Animator>();
        base.Start();
    }
}
