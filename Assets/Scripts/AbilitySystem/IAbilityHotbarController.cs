using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAbilityHotbarController
{
    IAbility GetAbilityFromSlot(int slotId);
    void UnSetAbilityFromSlot(int id);
    void UnSetAbilityFromSlot(IAbility ability);
    void SetAbilityOnSlot(IAbility ability, int slot);
    void InvokeGlobalCooldown(float time);
    void SetAbilitySystem(IAbilitySystem abilitySystem);
}
