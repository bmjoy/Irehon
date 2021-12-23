using System.Collections.Generic;
using UnityEngine;

public class MobDeathState : MobState
{
    public MobDeathState(Mob mob) : base(mob)
    {
    }

    private List<Collider> physicsColliders = new List<Collider>();
    public override void Enter()
    {
        this.physicsColliders.Clear();
        foreach (Collider collider in this.mob.GetComponents<Collider>())
        {
            if (!collider.isTrigger)
            {
                this.physicsColliders.Add(collider);
                collider.isTrigger = true;
            }
        }
        this.mob.GetComponent<MobMovement>().ResetDestination();
        this.mob.ChangeModelState(false);
    }

    public override void Exit()
    {
        foreach (Collider collider in this.physicsColliders)
        {
            collider.isTrigger = false;
        }
        this.mob.ChangeModelState(true);
    }

    public override MobState Update(float timeInState)
    {
        return this;
    }
}
