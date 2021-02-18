using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class AbilityBase : NetworkBehaviour, IAbility
{
    [SerializeField]
    protected float cooldownTime;
    [SerializeField]
    private float castTime;
    public KeyCode TriggerKey { get { return triggerOnKey; } }
    [SerializeField]
    private KeyCode triggerOnKey;
    private AbilitySystem abilitySystem;

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

    protected abstract void Ability(Vector3 target);

    protected abstract void InterruptAbility();
    
    protected void Start()
    {
        abilitySystem = GetComponent<AbilitySystem>();
        canCast = true;
        if (castTime > cooldownTime)
            cooldownTime = castTime;
    }


    public void Interrupt()
    {
        InterruptAbility();
        abilitySystem.AllowTrigger();
        if (isServer)
            InterruptOnOthers();
    }

    [ClientRpc(excludeOwner = true)]
    private void InterruptOnOthers()
    {
        InterruptAbility();
    }

    public void AnimationEvent() 
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

    [ClientRpc(excludeOwner = true)]
    private void StopHoldingAbilityOnClients(Vector3 target)
    {
        print("stop");
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

    [ClientRpc(excludeOwner = true)]
    protected void CastAbilityOnOthers(Vector3 target)
    {
        Ability(target);
    }

    protected void StartCooldown(float duration)
    {
        cooldownCoroutine = StartCoroutine(Cooldown());
        IEnumerator Cooldown()
        {
            canCast = false;
            yield return new WaitForSeconds(duration);
            canCast = true;
        }
    }

    protected void RemoveCooldown()
    {
        StopCoroutine(cooldownCoroutine);
        canCast = true;
    }
}
