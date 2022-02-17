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
        foreach (KeyValuePair<Entity, EntityCollider> entity in this.MeleeWeaponCollider.GetCollectedInZoneEntities())
        {
            this.mob.DoDamage(entity.Key, Mathf.RoundToInt(this.meleeDamage * entity.Value.damageMultiplier));
        }

        this.MeleeWeaponCollider.StopCollectColliders();
        this.MeleeWeaponCollider.StartCollectColliders();
        this.OnAttackEvent.Invoke();
    }
}
