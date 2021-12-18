using Irehon.Abilitys;
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
        Item currentWeapon = ItemDatabase.GetItemBySlug(this.slug);

        this.playerBonesLinks = abilitySystem.GetComponent<PlayerBonesLinks>();
        Vector3 localPos = this.transform.localPosition;
        Quaternion localRotation = this.transform.localRotation;
        this.transform.parent = this.playerBonesLinks.LeftHand;
        this.transform.localPosition = localPos;
        this.transform.localRotation = localRotation;
        this.ArrowInHand = Instantiate(this.arrowInHandPrefab, this.playerBonesLinks.RightHand);

        abilitySystem.animator.runtimeAnimatorController = this.animator;

        this.ability.SetArrowDamage(currentWeapon.metadata["Damage"].AsInt);
        this.ability.Setup(abilitySystem);

        SetAnimationSpeed(abilitySystem.animator, WeaponType.Bow, currentWeapon.metadata["AttackSpeed"].AsFloat);


        return this.ability;
    }

    public override WeaponType GetType()
    {
        return WeaponType.Bow;
    }

    public override void UnSetup(AbilitySystem abilitySystem)
    {
        Destroy(this.ArrowInHand);
        Destroy(this.gameObject);

        SetDefaultAnimationSpeed(abilitySystem.animator);
    }
}
