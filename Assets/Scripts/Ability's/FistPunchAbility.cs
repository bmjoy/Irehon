using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FistPunchAbility : AbilityBase
{
    [SerializeField]
    private GameObject handFistColliderPrefab;

    private MeleeWeaponCollider leftHandCollider;
    private MeleeWeaponCollider rightHandCollider;

    public override void Setup(AbilitySystem abilitySystem)
    {
        base.Setup(abilitySystem);
        leftHandCollider = Instantiate(handFistColliderPrefab, abilitySystem.PlayerBonesLinks.LeftHand).GetComponent<MeleeWeaponCollider>();
        rightHandCollider = Instantiate(handFistColliderPrefab, abilitySystem.PlayerBonesLinks.RightHand).GetComponent<MeleeWeaponCollider>();

        leftHandCollider.transform.localPosition = Vector3.zero;
        rightHandCollider.transform.localPosition = Vector3.zero;

        leftHandCollider.Intialize(abilitySystem.PlayerComponent.GetHitBoxColliderList());
        rightHandCollider.Intialize(abilitySystem.PlayerComponent.GetHitBoxColliderList());

        currentAnimationEvent = DamageEntitiesInArea;
    }
    protected override void Ability(Vector3 target)
    {
        abilitySystem.AnimatorComponent.SetTrigger("Skill1");
        AbilityStart();
    }

    private void DamageEntitiesInArea()
    {
        if (isLocalPlayer)
            CameraController.CreateShake(1, .3f);

        if (!isServer)
            return;

        foreach (var entity in leftHandCollider.GetHittableEntities())
            abilitySystem.PlayerComponent.DoDamage(entity.Key, Mathf.RoundToInt(GetDamage() * entity.Value.damageMultiplier));

        abilitySystem.AnimatorComponent.ResetTrigger("Skill1");
        AbilityEnd();
    }

    private int GetDamage()
    {
        return 20;
    }

    public void DestroyColliders()
    {
        Destroy(leftHandCollider.gameObject);
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
