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

    protected Coroutine cooldownCoroutine;
    protected AbilitySystem abilitySystem;
    protected CurrentAnimationEvent currentAnimationEvent;
    protected AbilityBase addingComponent;
    protected delegate void CurrentAnimationEvent();

    [SerializeField]
    protected int id;
    [SerializeField]
    protected float cooldownTime;
    [SerializeField]
    protected KeyCode triggerOnKey;

    private bool isCasting;
    protected virtual void Start()
    {
        abilitySystem = GetComponent<AbilitySystem>();
    }

    protected void AbilityEndEvent()
    {
        isCasting = false;
        abilitySystem.AllowTrigger();
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
}
