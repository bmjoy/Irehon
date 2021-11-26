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
        mob.ChangeModelState(false);
        foreach(var collider in mob.GetComponents<Collider>())
            collider.enabled = false;
        foreach (var collider in mob.HitboxColliders)
            collider.enabled = false;
        mob.GetComponent<MobMovement>().ResetDestination();
    }

    public override void Exit()
    {
        foreach (var collider in mob.GetComponents<Collider>())
            collider.enabled = true;
        foreach (var collider in mob.HitboxColliders)
            collider.enabled = true;
        mob.ChangeModelState(true);
    }

    public override MobState Update(float timeInState)
    {
        return this;
    }
}
