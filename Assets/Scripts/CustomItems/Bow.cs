using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : Weapon
{
    private PlayerBonesLinks playerBonesLinks;

    [SerializeField]
    private SniperAbility ability;

    public GameObject Arrow;
    public AudioClip TenseSound;
    public Transform BowStringBone;
    public ParticleSystem AimingParticles;
    [HideInInspector]
    public GameObject ArrowInHand;

    [SerializeField]
    private GameObject arrowInHandPrefab;

    public override AbilityBase Setup(AbilitySystem abilitySystem)
    {
        playerBonesLinks = abilitySystem.GetComponent<PlayerBonesLinks>();
        AimingParticles.transform.parent = abilitySystem.AbilityPoolObject.transform;
        ArrowInHand = Instantiate(arrowInHandPrefab, playerBonesLinks.RightHand);
        ability.Setup(abilitySystem);
        return ability;
    }

    private void OnDestroy()
    {
        Destroy(ArrowInHand);
        Destroy(AimingParticles);
    }
}
