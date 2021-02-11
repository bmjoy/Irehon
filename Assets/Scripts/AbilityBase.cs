using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class AbilityBase : NetworkBehaviour
{
    [SerializeField]
    private float cooldownTime;
    [SerializeField]
    private float castTime;
    [SerializeField]
    public KeyCode TriggerKey { get; private set; }
    private AbilitySystem abilitySystem;

    private bool canCast;

    protected virtual void Start()
    {
        abilitySystem = GetComponent<AbilitySystem>();
    }

    public virtual void TriggerKeyDown(Vector3 target)
    {
        if (canCast)
        {
            StartCooldown();
            Ability(target);
            abilitySystem.BlockTrigger();
            if (!isClientOnly)
                CastAbilityOnOthers(target);
        }
    }

    public virtual void TriggerKeyUp(Vector3 target)
    {
    }


    public virtual void Interrupt()
    {
        StopAllCoroutines();
        if (!isClientOnly)
            InterruptOnOthers();
    }

    [ClientRpc(excludeOwner = true)]
    private void InterruptOnOthers()
    {
        Interrupt();
    }

    protected abstract void Ability(Vector3 target);

    [ClientRpc(excludeOwner = true)]
    protected void CastAbilityOnOthers(Vector3 target)
    {
        Ability(target);
    }

    private void StartCooldown()
    {
        StartCoroutine(Cooldown());
        IEnumerator Cooldown()
        {
            canCast = false;
            yield return new WaitForSeconds(cooldownTime);
            canCast = true;
        }
    }
}
