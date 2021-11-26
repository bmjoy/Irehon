using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FistPunchAbility : AbilityBase
{
    [SerializeField]
    private GameObject handFistColliderPrefab;

    private MeleeWeaponCollider rightHandCollider;

    public override void Setup(AbilitySystem abilitySystem)
    {
        base.Setup(abilitySystem);
        rightHandCollider = Instantiate(handFistColliderPrefab, abilitySystem.PlayerBonesLinks.RightHand).GetComponent<MeleeWeaponCollider>();
        
        rightHandCollider.transform.localPosition = Vector3.zero;

        rightHandCollider.Intialize(abilitySystem.PlayerComponent.HitboxColliders);

        currentAnimationEvent = DamageEntitiesInArea;
    }
    protected override void Ability(Vector3 target)
    {
        abilitySystem.AnimatorComponent.SetTrigger("Skill1");
        rightHandCollider.StartCollectColliders();
        AbilityStart();
    }

    private void DamageEntitiesInArea()
    {
        if (isLocalPlayer)
            CameraController.CreateShake(1, .3f);

        if (!isServer)
            return;

        foreach (var entity in rightHandCollider.GetCollectedInZoneEntities())
            abilitySystem.PlayerComponent.DoDamage(entity.Key, Mathf.RoundToInt(GetDamage() * entity.Value.damageMultiplier));

        rightHandCollider.StopCollectColliders();

        abilitySystem.AnimatorComponent.ResetTrigger("Skill1");
        AbilityEnd();
    }

    private int GetDamage()
    {
        return 100;
    }

    public void DestroyColliders()
    {
        Destroy(rightHandCollider.gameObject);
    }

    protected override void InterruptAbility()
    {
        abilitySystem.AnimatorComponent.ResetTrigger("Skill1");
        AbilityEnd();
    }

    protected override void StopHoldingAbility(Vector3 target)
    {
    }
}
