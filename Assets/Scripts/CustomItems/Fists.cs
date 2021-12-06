using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fists : Weapon
{
    [SerializeField]
    private FistPunchAbility ability;
    [SerializeField]
    private AnimatorOverrideController animator;

    public override WeaponType GetType() => WeaponType.Fist;

    public override AbilityBase Setup(AbilitySystem abilitySystem)
    {
        abilitySystem.animator.runtimeAnimatorController = animator;
        ability.Setup(abilitySystem);
        return ability;
    }

    public override void UnSetup(AbilitySystem abilitySystem)
    {
        ability.DestroyColliders();
        Destroy(gameObject);
    }
}
