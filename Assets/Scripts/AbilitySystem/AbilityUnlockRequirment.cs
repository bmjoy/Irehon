using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class AbilityUnlockRequirment
{
    public SkillType RequiredSkillType { get; }
    public int RequiredSkillLvl { get; }
    public int[] RequiredUnlockedAbilityId { get; }

    public AbilityUnlockRequirment(SkillType skillType, int requiredSkilllvl, int[] requiredAbilityId)
    {
        RequiredSkillType = skillType;
        this.RequiredSkillLvl = requiredSkilllvl;
        RequiredUnlockedAbilityId = requiredAbilityId;
    }
}
