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
    public ParticleSystem AimingParticles;
    [HideInInspector]
    public GameObject ArrowInHand;

    [SerializeField]
    private GameObject arrowInHandPrefab;

    [SerializeField]
    private AnimatorOverrideController animator;

    public override AbilityBase Setup(AbilitySystem abilitySystem)
    {
        playerBonesLinks = abilitySystem.GetComponent<PlayerBonesLinks>();
        var localPos = transform.localPosition;
        var localRotation = transform.localRotation;
        transform.parent = playerBonesLinks.LeftHand;
        transform.localPosition = localPos;
        transform.localRotation = localRotation;
        AimingParticles.transform.parent = abilitySystem.AbilityPoolObject.transform;
        ArrowInHand = Instantiate(arrowInHandPrefab, playerBonesLinks.RightHand);

        abilitySystem.AnimatorComponent.runtimeAnimatorController = animator; 

        ability.Setup(abilitySystem);
        
        return ability;
    }

    public override WeaponType GetType() => WeaponType.Bow;

    public override void UnSetup(AbilitySystem abilitySystem)
    {
        Destroy(ArrowInHand);
        Destroy(AimingParticles);
        Destroy(gameObject);
    }
}
