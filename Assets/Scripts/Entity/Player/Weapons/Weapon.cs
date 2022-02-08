using Irehon.Abilitys;
using UnityEngine;

public enum WeaponType { Fist, Bow, TwoHandSword, TwoHandAxe, Sword, Dagger };
public abstract class Weapon : MonoBehaviour
{
    public string slug;
    public new abstract WeaponType GetType();

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

    public static int GetStaminaCost(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Sword:
                return 2000;
            case WeaponType.Dagger:
                return 1500;
            case WeaponType.Bow:
                return 2500;
            case WeaponType.TwoHandSword:
                return 2600;
            case WeaponType.TwoHandAxe:
                return 3000;
            default:
                return 1200;
        }
    }

    private static float GetRequiredSpeedModifier(WeaponType weaponType, float targetSpeed)
    {
        return GetAnimationLength(weaponType) / targetSpeed;
    }

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
