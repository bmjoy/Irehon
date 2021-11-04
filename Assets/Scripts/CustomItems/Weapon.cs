using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType { Fist, Bow, TwoHandSword, TwoHandAxe, Sword, Dagger };
public abstract class Weapon : MonoBehaviour
{
    new public abstract WeaponType GetType();

    private static float GetAnimationLength(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Sword:
                return 0.8f;
            case WeaponType.Dagger:
                return 1f;
            case WeaponType.Bow:
                return 1f;
            case WeaponType.TwoHandSword:
                return 1.2f;
            case WeaponType.TwoHandAxe:
                return 1.6f;
            default:
                return 1f;
        }
    }

    private static float GetRequiredSpeedModifier(WeaponType weaponType, float targetSpeed) => targetSpeed / GetAnimationLength(weaponType);
    protected static void SetAnimationSpeed(Animator animator, WeaponType type, float targetSpeed)
    {
        animator.SetFloat("AnimationSpeed", GetRequiredSpeedModifier(type, targetSpeed));
    }
    protected static void SetDefaultAnimationSpeed(Animator animator)
    {
        animator.SetFloat("AnimationSpeed", 1f);
    }
    public abstract AbilityBase Setup(AbilitySystem abilitySystem);
    public abstract void UnSetup(AbilitySystem abilitySystem);
}
