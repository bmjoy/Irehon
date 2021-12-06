using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FistPunchAbility : AbilityBase
{
    [SerializeField]
    private GameObject handFistColliderPrefab;

    [SerializeField]
    private AudioClip onAttackStartSound;
    [SerializeField]
    private AudioClip onImpactSound;

    private MeleeWeaponCollider leftHandCollider;

    public override void Setup(AbilitySystem abilitySystem)
    {
        base.Setup(abilitySystem);
        leftHandCollider = Instantiate(handFistColliderPrefab, abilitySystem.playerBonesLinks.LeftHand).GetComponent<MeleeWeaponCollider>();
        
        leftHandCollider.transform.localPosition = Vector3.zero;

        leftHandCollider.Intialize(abilitySystem.player.HitboxColliders);

        currentAnimationEvent = DamageEntitiesInArea;
    }
    protected override void Ability(Vector3 target)
    {
        abilitySystem.animator.SetTrigger("Skill1");
        leftHandCollider.StartCollectColliders();
        AbilityStart();
    }

    public override void AbilitySoundEvent()
    {
        abilitySystem.PlaySoundClip(onAttackStartSound);
    }
    private void DamageEntitiesInArea()
    {
        if (isLocalPlayer)
            CameraController.CreateShake(1, .3f);

        if (leftHandCollider.GetCollectedInZoneEntities().Count != 0)
            abilitySystem.PlaySoundClip(onImpactSound);

        if (!isServer)
            return;

        foreach (var entity in leftHandCollider.GetCollectedInZoneEntities())
            abilitySystem.player.DoDamage(entity.Key, Mathf.RoundToInt(GetDamage() * entity.Value.damageMultiplier));

        leftHandCollider.StopCollectColliders();

        abilitySystem.animator.ResetTrigger("Skill1");
        AbilityEnd();
    }

    private int GetDamage()
    {
        return 100;
    }

    public void DestroyColliders()
    {
        Destroy(leftHandCollider.gameObject);
    }

    protected override void InterruptAbility()
    {
        abilitySystem.animator.ResetTrigger("Skill1");
        AbilityEnd();
    }

    protected override void StopHoldingAbility(Vector3 target)
    {
    }
}
