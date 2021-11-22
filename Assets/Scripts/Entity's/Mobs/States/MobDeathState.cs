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
        mob.Model.gameObject.SetActive(false);
    }

    public override void Exit()
    {
        mob.Model.gameObject.SetActive(true);
    }

    public override MobState Update(float timeInState)
    {
        return this;
    }
}
