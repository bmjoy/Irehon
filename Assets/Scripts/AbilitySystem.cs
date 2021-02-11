using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySystem : MonoBehaviour, IAbilitySystem
{
    [SerializeField]
    private AbilityBase[] abilitys;

    private AbilityBase currentlyCastingSkill;

    private bool canTriggerAbility;

    public void AbilityKeyDown(KeyCode key, Vector3 target)
    {
        if (!canTriggerAbility)
            return;
        foreach (AbilityBase ability in abilitys)
        {
            if (ability.TriggerKey == key)
                ability.TriggerKeyDown(target);
        }
    }

    public void AbilityKeyUp(KeyCode key, Vector3 target)
    {
        if (!canTriggerAbility)
            return;
        foreach (AbilityBase ability in abilitys)
        {
            if (ability.TriggerKey == key)
                ability.TriggerKeyUp(target);
        }
    }

    public void AllowTrigger()
    {
        canTriggerAbility = true;
    }

    public void BlockTrigger()
    {
        canTriggerAbility = false;
    }
}
