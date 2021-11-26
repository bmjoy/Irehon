using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MobDeathState : MobState
{
    public MobDeathState(Mob mob) : base(mob)
    {
    }
    public override void Enter()
    {
        mob.GetComponent<MobMovement>().ResetDestination();
        mob.ChangeModelState(false);
    }

    public override void Exit()
    {
        mob.ChangeModelState(true);
    }

    public override MobState Update(float timeInState)
    {
        return this;
    }
}
