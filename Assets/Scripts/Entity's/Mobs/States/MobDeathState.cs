using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MobDeathState : MobState
{
    public MobDeathState(Mob mob) : base(mob)
    {
    }

    private List<Collider> physicsColliders = new List<Collider>();
    public override void Enter()
    {
        physicsColliders.Clear();
        foreach (Collider collider in mob.GetComponents<Collider>())
        {
            if (!collider.isTrigger)
            {
                physicsColliders.Add(collider);
                collider.isTrigger = true;
            }
        }
        mob.GetComponent<MobMovement>().ResetDestination();
        mob.ChangeModelState(false);
    }

    public override void Exit()
    {
        foreach (Collider collider in physicsColliders)
        {
            collider.isTrigger = false;
        }
        mob.ChangeModelState(true);
    }

    public override MobState Update(float timeInState)
    {
        return this;
    }
}
