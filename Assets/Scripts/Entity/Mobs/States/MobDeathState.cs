using System.Collections.Generic;
using UnityEngine;

public class MobDeathState : MobState
{
    public MobDeathState(Mob mob) : base(mob)
    {
    }

    public override void Enter()
    {
        this.mob.GetComponent<MobMovement>().ResetDestination();
    }

    public override void Exit()
    {
    }

    public override MobState Update(float timeInState)
    {
        return this;
    }
}
