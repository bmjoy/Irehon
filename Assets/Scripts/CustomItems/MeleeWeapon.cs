using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum HandToLink { Left, Right}

public class MeleeWeapon : Weapon
{
    [SerializeField]
    private HandToLink hand = HandToLink.Right;
    [SerializeField]
    private WeaponType type;
    [SerializeField]
    private AnimatorOverrideController animatorOverrideController;

    public override WeaponType GetType() => type;
    public override AbilityBase Setup(AbilitySystem abilitySystem)
    {
        Item currentWeapon = ItemDatabase.GetItemBySlug(slug);

        var playerBonesLinks = abilitySystem.GetComponent<PlayerBonesLinks>();

        var localScale = transform.localScale;
        var localPos = transform.localPosition;
        var localRotation = transform.localRotation;
        if (hand == HandToLink.Right)
            transform.parent = playerBonesLinks.RightHand;
        else
            transform.parent = playerBonesLinks.LeftHand;
        transform.localPosition = localPos;
        transform.localRotation = localRotation;
        transform.localScale = localScale;

        abilitySystem.AnimatorComponent.runtimeAnimatorController = animatorOverrideController;

        GetComponent<AbilityBase>().Setup(abilitySystem);

        GetComponent<MeleeWeaponAbility>().SetDamage(currentWeapon.metadata["Damage"].AsInt);

        SetAnimationSpeed(abilitySystem.AnimatorComponent, type, currentWeapon.metadata["AttackSpeed"].AsFloat);

        return GetComponent<AbilityBase>();
    }

    public override void UnSetup(AbilitySystem abilitySystem)
    {
        Destroy(gameObject);

        SetDefaultAnimationSpeed(abilitySystem.AnimatorComponent);
    }
}
