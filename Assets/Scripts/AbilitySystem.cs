using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySystem : MonoBehaviour, IAbilitySystem
{
    [SerializeField]
    private AbilityBase[] abilitysPool;

    private IAbility[] abilitys;

    private IAbility currentlyCastingSkill;

    private bool canTriggerAbility;

    private void Start()
    {
        canTriggerAbility = true;
         abilitys = abilitysPool;
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
                    currentlyCastingSkill = ability;
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

    public void AllowTrigger()
    {
        canTriggerAbility = true;
        currentlyCastingSkill = null;
    }

    public void BlockTrigger()
    {
        canTriggerAbility = false;
    }

    public void AnimationEventTrigger()
    {
        if (currentlyCastingSkill != null)
            currentlyCastingSkill.AnimationEvent();
    }
}
