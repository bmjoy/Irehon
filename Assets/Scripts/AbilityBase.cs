using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using System.Reflection;

public class AbilityCooldownEvent : UnityEvent<float> {}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class CopyFieldAttribute : Attribute { }

public abstract class AbilityBase : NetworkBehaviour, IAbility
{
    public KeyCode TriggerKey { get => triggerOnKey; }
    public Sprite AbilityIcon { get => icon; }
    public AbilityCooldownEvent OnAbilityCooldown { get => onAbilityCooldown ; set => onAbilityCooldown = value; }

    private AbilityCooldownEvent onAbilityCooldown = new AbilityCooldownEvent();

    [SerializeField]
    protected float cooldownTime;
    [SerializeField]
    protected KeyCode triggerOnKey;
    [SerializeField]
    private Sprite icon;

    protected AbilitySystem abilitySystem;
    public abstract int Id { get; }
    protected delegate void CurrentAnimationEvent();
    protected CurrentAnimationEvent currentAnimationEvent;

    private bool canCast;
    private bool isCasting;

    protected Coroutine cooldownCoroutine;

    protected void AbilityEndEvent()
    {
        isCasting = false;
        abilitySystem.AllowTrigger();
    }

    protected abstract void StopHoldingAbility(Vector3 target);



    protected AbilityBase addingComponent;
    protected abstract void InterruptAbility();
    
    protected virtual void Start()
    {
        canCast = true;
        abilitySystem = GetComponent<AbilitySystem>();
    }

    public void Interrupt()
    {
        InterruptAbility();
        abilitySystem.AllowTrigger();
        if (isServer)
            InterruptOnOthers();
    }

    [ClientRpc]
    private void InterruptOnOthers()
    {
        InterruptAbility();
    }

    public void AnimationEvent()
    {
        if (currentAnimationEvent != null)
        {
            currentAnimationEvent();
            AnimationEventOnOthers();
        }
    }

    [ClientRpc]
    public void AnimationEventOnOthers()
    {
        if (currentAnimationEvent != null)
            currentAnimationEvent();
    }

    public void TriggerKeyUp(Vector3 target)
    {
        if (isCasting)
        {
            StopHoldingAbility(target);
            if (isServer)
                StopHoldingAbilityOnClients(target);
        }
    }

    [ClientRpc]
    private void StopHoldingAbilityOnClients(Vector3 target)
    {
        StopHoldingAbility(target);
    }
    
    public bool TriggerKeyDown(Vector3 target)
    {
        if (canCast)
        {
            isCasting = true;
            StartCooldown(cooldownTime);
            Ability(target);
            abilitySystem.BlockTrigger();
            if (!isClientOnly)
                CastAbilityOnOthers(target);
            return true;
        }
        return false;
    }
    protected abstract void Ability(Vector3 target);

    [ClientRpc]
    protected void CastAbilityOnOthers(Vector3 target)
    {
        Ability(target);
    }

    [TargetRpc]
    protected void ConfirmTrigger(NetworkConnection con, Vector3 target)
    {
        abilitySystem.BlockTrigger();
        isCasting = true;
        StartCooldown(cooldownTime);
    }

    [Server]
    protected void StartCooldown(float duration)
    {
        OnAbilityCooldown.Invoke(duration);
        cooldownCoroutine = StartCoroutine(Cooldown());
        StartCooldownOnOthers(connectionToClient, duration);
        IEnumerator Cooldown()
        {
            canCast = false;
            yield return new WaitForSeconds(duration);
            canCast = true;
        }
    }

    [TargetRpc]
    protected void StartCooldownOnOthers(NetworkConnection con, float duration)
    {
        OnAbilityCooldown.Invoke(duration);
    }

    protected void RemoveCooldown()
    {
        StopCoroutine(cooldownCoroutine);
        canCast = true;
    }
}
