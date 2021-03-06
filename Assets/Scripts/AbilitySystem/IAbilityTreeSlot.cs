using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IAbilityTreeSlot
{
    int SkillId { get; }
    void SetAbilityInfo(IAbility ability);
    void SetState(bool state);
}