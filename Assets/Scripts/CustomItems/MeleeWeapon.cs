using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MeleeWeapon : Weapon
{
    private static float GetAnimationLength(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Sword:
                return 1.2f;
            case WeaponType.Dagger:
                return 1.3f;
            case WeaponType.Bow:
                return 1.5f;
            case WeaponType.TwoHandSword:
                return 1.3f;
            case WeaponType.TwoHandAxe:
                return 1.1f;
            default:
                return 1f;
        }
    }

    private static float GetRequiredSPeedModifier(WeaponType weaponType, float targetSpeed) => targetSpeed / GetAnimationLength(weaponType);

    [SerializeField]
    private WeaponType type;
    [SerializeField]
    private AnimatorOverrideController animatorOverrideController;

    private AbilityBase ability;

    public override WeaponType GetType() => type;
    public override AbilityBase Setup(AbilitySystem abilitySystem)
    {
        Item currentWeapon = ItemDatabase.GetItemBySlug(gameObject.name);

        var playerBonesLinks = abilitySystem.GetComponent<PlayerBonesLinks>();

        var localScale = transform.localScale;
        var localPos = transform.localPosition;
        var localRotation = transform.localRotation;
        transform.parent = playerBonesLinks.RightHand;
        transform.localPosition = localPos;
        transform.localRotation = localRotation;
        transform.localScale = localScale;

        abilitySystem.AnimatorComponent.runtimeAnimatorController = animatorOverrideController;

        GetComponent<AbilityBase>().Setup(abilitySystem);

        GetComponent<MeleeWeaponAbility>().SetDamage(currentWeapon.metadata["Attack"].AsInt);

        return GetComponent<AbilityBase>();
    }

    public override void UnSetup(AbilitySystem abilitySystem)
    {
        Destroy(gameObject);
    }
}
