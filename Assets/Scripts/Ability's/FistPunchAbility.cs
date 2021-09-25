using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FistPunchAbility : AbilityBase
{
    public override void Setup(AbilitySystem abilitySystem)
    {
        base.Setup(abilitySystem);
        print("setuped"); ;
    }
    protected override void Ability(Vector3 target)
    {
        print("Punch");
    }

    protected override void InterruptAbility()
    {
        print("Punch interrupt");
    }

    protected override void StopHoldingAbility(Vector3 target)
    {
    }
}
