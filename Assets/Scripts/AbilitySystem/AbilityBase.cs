using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using System.Reflection;

public abstract class AbilityBase : MonoBehaviour
{
    public KeyCode TriggerKey { get => triggerOnKey; }

    protected bool isLocalPlayer { get => abilitySystem.isLocalPlayer; }
    protected bool isServer { get => abilitySystem.isServer; }

    protected Coroutine cooldownCoroutine;
    protected AbilitySystem abilitySystem;
    protected PlayerBonesLinks boneLinks;
    protected Weapon weapon;

    protected CurrentAnimationEvent currentAnimationEvent;
    protected delegate void CurrentAnimationEvent();

    [SerializeField]
    protected float cooldownTime;
    [SerializeField]
    protected KeyCode triggerOnKey;

    public virtual void Setup(AbilitySystem abilitySystem)
    {
        this.abilitySystem = abilitySystem;
        boneLinks = abilitySystem.playerBonesLinks;
        weapon = GetComponent<Weapon>();
    }

    protected abstract void StopHoldingAbility(Vector3 target);
    protected abstract void Ability(Vector3 target);
    protected abstract void InterruptAbility();

    public void Interrupt()
    {
        InterruptAbility();
    }

    public void AnimationEvent()
    {
        if (currentAnimationEvent != null)
        {
            currentAnimationEvent();
        }
    }

    public void TriggerKeyUp(Vector3 target)
    {
        StopHoldingAbility(target);
    }
    
    public bool TriggerKeyDown(Vector3 target)
    {
        Ability(target);
        return true;
    }

    public virtual void AbilitySoundEvent()
    {

    }

    public virtual void SubEvent()
    {

    }

    protected void AbilityStart()
    {
        abilitySystem.BlockTrigger();
    }

    protected void AbilityEnd()
    {
        abilitySystem.AllowTrigger();
    }
}
