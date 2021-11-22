using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MobWanderState : MobState
{
    public MobWanderState(Mob mob) : base(mob) 
    {
        mobMovement = mob.GetComponent<MobMovement>();
    }

    protected MobMovement mobMovement;

    public override void Enter()
    {
        mobMovement.Wander();
        mob.GetComponent<Animator>().SetBool("isWalking", true);
    }

    public override void Exit()
    {
        mob.GetComponent<Animator>().SetBool("isWalking", false);
        mobMovement.ResetDestination();
    }

    public override MobState Update(float timeInState)
    {
        if (mobMovement.IsDestinationReached || timeInState > 10f)
            return new MobIdleState(mob);
        return this;
    }
}
