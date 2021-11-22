using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MobFearState : MobState
{
    public MobFearState(Mob mob) : base(mob)
    {
        mobMovement = mob.GetComponent<MobMovement>();
    }

    protected MobMovement mobMovement;

    public override void Enter()
    {
        mobMovement.SetSpeed(mobMovement.RunSpeed);
        mobMovement.Wander();
        Debug.Log("Fear");
        mob.GetComponent<Animator>().SetBool("isRunning", true);
    }

    public override void Exit()
    {
        mobMovement.SetSpeed(mobMovement.WalkingSpeed);
        mob.GetComponent<Animator>().SetBool("isRunning", false);
        mobMovement.ResetDestination();
    }

    public override MobState Update(float timeInState)
    {
        if (timeInState > 40f)
            return new MobIdleState(mob);

        if (mobMovement.IsDestinationReached)
            mobMovement.Wander();
        
        return this;
    }
}
