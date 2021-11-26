using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MobAgressiveAttackState : MobState
{
    public MobAgressiveAttackState(AggressiveMob mob) : base(mob)
    {
        this.mob = mob;
        mobMovement = mob.GetComponent<MobMovement>();

    }
    protected MobMovement mobMovement;
    protected new AggressiveMob mob;
    public override void Enter()
    {
        mob.GetComponent<Animator>().SetBool("isAttacking", true);
        mob.GetComponent<MobAnimationAttackHolder>().MeleeWeaponCollider.StartCollectColliders();

    }

    public override void Exit()
    {
        mob.GetComponent<Animator>().SetBool("isAttacking", false);
        mob.GetComponent<MobAnimationAttackHolder>().MeleeWeaponCollider.StopCollectColliders();
    }

    public override MobState Update(float timeInState)
    {
        if (mob.target == null)
            return new MobIdleState(mob);

        if (Vector3.Distance(mob.transform.position, mob.target.transform.position) > mob.RequiredAttackDistance)
            return new MobAgressiveWanderState(mob);
        
        mobMovement.SetDestination(mob.target.transform.position);
        
        return this;
    }
}
