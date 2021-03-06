using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAbilitySlot
{
    void Intialize(int slotId, IAbilitySystem abilitySystem);
    void SetAbility(IAbility ability);
    void UnSetCurrentAbility();
    void StartCooldown(float cooldownTime);
    void GlobalCooldown(float time);
    IAbility CurrentAbility { get; }
}
