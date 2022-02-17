using Irehon.Abilitys;
using UnityEngine;

public class Fists : Weapon
{
    [SerializeField]
    private FistPunchAbility ability;
    [SerializeField]
    private AnimatorOverrideController animator;

    public override WeaponType GetType()
    {
        return WeaponType.Fist;
    }

    public override AbilityBase Setup(AbilitySystem abilitySystem)
    {
        abilitySystem.animator.runtimeAnimatorController = this.animator;
        this.ability.Setup(abilitySystem);
        return this.ability;
    }

    public override void UnSetup(AbilitySystem abilitySystem)
    {
        this.ability.DestroyColliders();
        Destroy(this.gameObject);
    }
}
