using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

[RequireComponent(typeof(MeleeWeaponCollider))]
public class MeleeWeaponAbility : AbilityBase
{
    public UnityEvent OnClientAttackStart;

    private MeleeWeaponCollider meleeCollider;
    [SerializeField]
    private AudioClip onAttackStartSound;
    [SerializeField]
    private AudioClip onImpactSound;

    private int damage;

    public override void Setup(AbilitySystem abilitySystem)
    {
        base.Setup(abilitySystem);

        meleeCollider = GetComponent<MeleeWeaponCollider>();
        meleeCollider.Intialize(abilitySystem.player.HitboxColliders);

        currentAnimationEvent = DamageEntitiesInArea;

        abilitySystem.player.OnDoDamageEvent.AddListener(ImpactSound);

        meleeCollider.OnNewCollectedEntityEvent.AddListener(DamageEntity);
    }
    protected override void Ability(Vector3 target)
    {
        abilitySystem.animator.SetTrigger("Skill1");
        AbilityStart();
        if (abilitySystem.isClient)
            OnClientAttackStart.Invoke();
    }

    public override void SubEvent()
    {
        meleeCollider.StartCollectColliders();
    }

    public override void AbilitySoundEvent()
    {
        abilitySystem.PlaySoundClip(onAttackStartSound);
    }

    private void DamageEntitiesInArea()
    {
        if (isLocalPlayer)
            CameraController.CreateShake(1, .3f);

        if (!isServer)
            return;

        meleeCollider.StopCollectColliders();

        abilitySystem.animator.ResetTrigger("Skill1");
        AbilityEnd();
    }

    private void DamageEntity(Entity entity, EntityCollider entityCollider)
    {
        abilitySystem.player.DoDamage(entity, Mathf.RoundToInt(GetDamage() * entityCollider.damageMultiplier));
    }

    private void ImpactSound(int damage)
    {
        if (damage != 0)
            abilitySystem.PlaySoundClip(onImpactSound);
    }

    private int GetDamage()
    {
        return damage;
    }

    public int SetDamage(int damage) => this.damage = damage;

    protected override void InterruptAbility()
    {
        abilitySystem.animator.ResetTrigger("Skill1");
        AbilityEnd();
    }

    protected override void StopHoldingAbility(Vector3 target)
    {
    }
}
