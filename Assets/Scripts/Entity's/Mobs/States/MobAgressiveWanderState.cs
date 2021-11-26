using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MobAgressiveWanderState : MobState
{
    public MobAgressiveWanderState(AggressiveMob mob) : base(mob)
    {
        this.mob = mob;
        mobMovement = mob.GetComponent<MobMovement>();
    }
    protected new AggressiveMob mob;

    protected MobMovement mobMovement;
    public override void Enter()
    {
        mob.GetComponent<Animator>().SetBool("isRunning", true);
        mobMovement.SetSpeed(mobMovement.RunSpeed);
    }
    public override void Exit()
    {
        mob.GetComponent<Animator>().SetBool("isRunning", false);
        mobMovement.SetSpeed(mobMovement.WalkingSpeed);
        mobMovement.ResetDestination();
    }

    public override MobState Update(float timeInState)
    {
        //Is destroyed
        if (mob.target == null)
            return new MobIdleState(mob);

        //Is runned away
        if (timeInState > 15 && Vector3.Distance(mob.transform.position, mob.target.transform.position) > mob.UnagroRadius)
        {
            mob.UnAgro();
            return new MobIdleState(mob);
        }

        if (Vector3.Distance(mob.transform.position, mob.SpawnPosition) > mob.AvoidRadius)
        {
            mob.SetDefaultState();
            return new MobIdleState(mob);
        }
        if (Vector3.Distance(mob.transform.position, mob.target.transform.position) < mob.RequiredAttackDistance)
            return new MobAgressiveAttackState(mob);

        mobMovement.SetDestination(mob.target.transform.position);

        return this;
    }
}
