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
        mob.IsModelShown = false;
        foreach(var collider in mob.GetComponents<Collider>())
            collider.enabled = false;
        mob.GetComponent<MobMovement>().ResetDestination();
    }

    public override void Exit()
    {
        foreach (var collider in mob.GetComponents<Collider>())
            collider.enabled = true;
        mob.IsModelShown = true;
    }

    public override MobState Update(float timeInState)
    {
        return this;
    }
}
