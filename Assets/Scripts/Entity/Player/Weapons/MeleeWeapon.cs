using Irehon.Abilitys;
using UnityEngine;

public enum HandToLink { Left, Right }

public class MeleeWeapon : Weapon
{
    [SerializeField]
    private HandToLink hand = HandToLink.Right;
    [SerializeField]
    private WeaponType type;
    [SerializeField]
    private AnimatorOverrideController animatorOverrideController;

    public override WeaponType GetType()
    {
        return this.type;
    }

    public override AbilityBase Setup(AbilitySystem abilitySystem)
    {
        Item currentWeapon = ItemDatabase.GetItemBySlug(this.slug);

        PlayerBonesLinks playerBonesLinks = abilitySystem.GetComponent<PlayerBonesLinks>();

        Vector3 localScale = this.transform.localScale;
        Vector3 localPos = this.transform.localPosition;
        Quaternion localRotation = this.transform.localRotation;
        if (this.hand == HandToLink.Right)
        {
            this.transform.parent = playerBonesLinks.RightHand;
        }
        else
        {
            this.transform.parent = playerBonesLinks.LeftHand;
        }

        this.transform.localPosition = localPos;
        this.transform.localRotation = localRotation;
        this.transform.localScale = localScale;

        abilitySystem.animator.runtimeAnimatorController = this.animatorOverrideController;

        this.GetComponent<AbilityBase>().Setup(abilitySystem);

        this.GetComponent<MeleeWeaponAbility>().SetDamage(currentWeapon.metadata["Damage"].AsInt);

        SetAnimationSpeed(abilitySystem.animator, this.type, currentWeapon.metadata["AttackSpeed"].AsFloat);

        return this.GetComponent<AbilityBase>();
    }

    public override void UnSetup(AbilitySystem abilitySystem)
    {
        Destroy(this.gameObject);

        SetDefaultAnimationSpeed(abilitySystem.animator);
    }
}
