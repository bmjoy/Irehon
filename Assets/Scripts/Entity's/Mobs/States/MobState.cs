using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class MobState
{
    public MobState(Mob mob)
    {
        this.mob = mob;
    }

    protected Mob mob;

    public abstract void Enter();
    public abstract void Exit();

    public abstract MobState Update(float timeInState);
}
