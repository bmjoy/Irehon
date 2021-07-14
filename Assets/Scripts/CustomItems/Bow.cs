using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : Weapon
{
    private AbilityPrefabData prefabData;

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
        prefabData = abilitySystem.GetComponent<AbilityPrefabData>();
        AimingParticles.transform.parent = abilitySystem.AbilityPoolObject.transform;
        ArrowInHand = Instantiate(arrowInHandPrefab, prefabData.RightHand);
        ability.Setup(abilitySystem);
        return ability;
    }

    private void OnDestroy()
    {
        Destroy(ArrowInHand);
        Destroy(AimingParticles);
    }
}
