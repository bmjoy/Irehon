using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : Weapon
{
    private PlayerBonesLinks playerBonesLinks;

    [SerializeField]
    private HoldReleaseArrowAbility ability;

    public GameObject Arrow;
    public AudioClip TenseSound;
    public Transform BowStringBone;
    [HideInInspector]
    public GameObject ArrowInHand;

    [SerializeField]
    private GameObject arrowInHandPrefab;

    [SerializeField]
    private AnimatorOverrideController animator;

    public override AbilityBase Setup(AbilitySystem abilitySystem)
    {
        Item currentWeapon = ItemDatabase.GetItemBySlug(slug);

        playerBonesLinks = abilitySystem.GetComponent<PlayerBonesLinks>();
        var localPos = transform.localPosition;
        var localRotation = transform.localRotation;
        transform.parent = playerBonesLinks.LeftHand;
        transform.localPosition = localPos;
        transform.localRotation = localRotation;
        ArrowInHand = Instantiate(arrowInHandPrefab, playerBonesLinks.RightHand);

        abilitySystem.animator.runtimeAnimatorController = animator;

        ability.SetArrowDamage(currentWeapon.metadata["Damage"].AsInt);
        ability.Setup(abilitySystem);

        SetAnimationSpeed(abilitySystem.animator, WeaponType.Bow, currentWeapon.metadata["AttackSpeed"].AsFloat);


        return ability;
    }

    public override WeaponType GetType() => WeaponType.Bow;

    public override void UnSetup(AbilitySystem abilitySystem)
    {
        Destroy(ArrowInHand);
        Destroy(gameObject);

        SetDefaultAnimationSpeed(abilitySystem.animator);
    }
}
