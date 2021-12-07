using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
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
        mob = GetComponent<AggressiveMob>();
        MeleeWeaponCollider.Intialize(mob.HitboxColliders);
    }

    public virtual void AttackEvent()
    {
        foreach (var entity in MeleeWeaponCollider.GetCollectedInZoneEntities())
            mob.DoDamage(entity.Key, Mathf.RoundToInt(meleeDamage * entity.Value.damageMultiplier));
        MeleeWeaponCollider.StopCollectColliders();
        MeleeWeaponCollider.StartCollectColliders();
        OnAttackEvent.Invoke();
    }
}
