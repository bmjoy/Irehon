using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MeleeWeaponAbility : AbilityBase
{
    [SerializeField]
    private MeleeWeaponCollider swordCollider;

    private int damage;

    public override void Setup(AbilitySystem abilitySystem)
    {
        base.Setup(abilitySystem);

        swordCollider.Intialize(abilitySystem.PlayerComponent.GetHitBoxColliderList());

        currentAnimationEvent = DamageEntitiesInArea;
    }
    protected override void Ability(Vector3 target)
    {
        abilitySystem.AnimatorComponent.SetTrigger("Skill1");
        AbilityStart();
    }

    private void DamageEntitiesInArea()
    {
        if (isLocalPlayer)
            CameraController.CreateShake(1, .3f);

        if (!isServer)
            return;

        foreach (var entity in swordCollider.GetHittableEntities())
            abilitySystem.PlayerComponent.DoDamage(entity.Key,  Mathf.RoundToInt(GetDamage() * entity.Value.damageMultiplier));

        abilitySystem.AnimatorComponent.ResetTrigger("Skill1");
        AbilityEnd();
    }

    private int GetDamage()
    {
        return damage;
    }

    public int SetDamage(int damage) => this.damage = damage;

    protected override void InterruptAbility()
    {
        abilitySystem.AnimatorComponent.ResetTrigger("Skill1");
        AbilityEnd();
    }

    protected override void StopHoldingAbility(Vector3 target)
    {
    }
}
