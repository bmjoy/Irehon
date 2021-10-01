using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MeleeWeapon : Weapon
{
    [SerializeField]
    private WeaponType type;
    [SerializeField]
    private AnimatorOverrideController animatorOverrideController;

    private AbilityBase ability;

    public override WeaponType GetType() => type;
    public override AbilityBase Setup(AbilitySystem abilitySystem)
    {
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

        return GetComponent<AbilityBase>();
    }

    public override void UnSetup(AbilitySystem abilitySystem)
    {
        Destroy(gameObject);
    }
}
