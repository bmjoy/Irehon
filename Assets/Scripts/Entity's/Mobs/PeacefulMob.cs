using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PeacefulMob : Mob
{
    protected override void Start()
    {
        base.Start();
        if (isServer)
            OnTakeDamageEvent += (dmg => 
            { 
                if (isAlive)
                    stateMachine.SetNewState(new MobFearState(this)); 
            });
    }
}
