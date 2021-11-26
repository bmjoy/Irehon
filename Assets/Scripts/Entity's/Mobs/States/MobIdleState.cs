using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MobIdleState : MobState
{
    public MobIdleState(Mob mob) : base(mob) 
    {
        wanderTimer = Random.Range(1, 7);
    }
    private float wanderTimer;
    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    public override MobState Update(float timeInState)
    {
        if (timeInState > wanderTimer)
            return new MobWanderState(mob);

        return this;
    }
}
