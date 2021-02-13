using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class AbilitySystem : NetworkBehaviour, IAbilitySystem
{
    [SerializeField]
    private List<AbilityBase> abilitysPool = new List<AbilityBase>();

    private List<IAbility> abilitys = new List<IAbility>();

    private IAbility currentlyCastingSkill;

    private bool canTriggerAbility;

    private void Start()
    {
        canTriggerAbility = true;
        foreach (IAbility ability in abilitysPool)
            abilitys.Add(ability);
    }

    public void AbilityKeyDown(KeyCode key, Vector3 target)
    {
        if (!canTriggerAbility || currentlyCastingSkill != null)
            return;
        foreach (IAbility ability in abilitys)
        {
            if (ability.TriggerKey == key)
            {
                if (ability.TriggerKeyDown(target))
                {
                    SetCurrentSkill(ability);
                }
            }
        }
    }

    public void AbilityKeyUp(KeyCode key, Vector3 target)
    {
        foreach (IAbility ability in abilitys)
        {
            if (ability.TriggerKey == key && currentlyCastingSkill == ability)
            {
                ability.TriggerKeyUp(target);
            }
        }
    }

    private void SetCurrentSkill(IAbility ability)
    {
        currentlyCastingSkill = ability;
        if (isServer)
        {
            if (ability == null)
                SetCurrentCastingSkillOnOthers(-1);
            else
                SetCurrentCastingSkillOnOthers(abilitys.IndexOf(ability));
        }
    }

    [ClientRpc(excludeOwner = true)]
    private void SetCurrentCastingSkillOnOthers(int skillIndex)
    {
        if (skillIndex >= 0 && skillIndex < abilitys.Count)
            currentlyCastingSkill = abilitys[skillIndex];
        else
            currentlyCastingSkill = null;
    }

    public void AllowTrigger()
    {
        canTriggerAbility = true;
        SetCurrentSkill(null);
    }

    public void BlockTrigger()
    {
        canTriggerAbility = false;
    }

    public void AnimationEventTrigger()
    {
        print("evenbt");
        if (currentlyCastingSkill != null)
            currentlyCastingSkill.AnimationEvent();
    }
}
