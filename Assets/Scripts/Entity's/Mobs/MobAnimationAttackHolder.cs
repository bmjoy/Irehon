using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MobAnimationAttackHolder : MonoBehaviour
{
    [SerializeField]
    protected int meleeDamage;
    public MeleeWeaponCollider MeleeWeaponCollider;
    protected AggressiveMob mob;

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
    }
}
