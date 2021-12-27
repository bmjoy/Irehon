using Irehon.Entitys;
using Irehon.Entitys;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MobAnimationAttackHolder : MonoBehaviour
{
    [SerializeField]
    protected int meleeDamage;
    public MeleeWeaponCollider MeleeWeaponCollider;
    protected AggressiveMob mob;
    public UnityEvent OnAttackEvent;

    private void Awake()
    {
        this.mob = this.GetComponent<AggressiveMob>();
        this.MeleeWeaponCollider.Intialize(this.mob.HitboxColliders);
    }

    public virtual void AttackEvent()
    {
        this.MeleeWeaponCollider.StopCollectColliders();
        this.MeleeWeaponCollider.StartCollectColliders();
        this.OnAttackEvent.Invoke();
    }
}
