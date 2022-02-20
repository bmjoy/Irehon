using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;


class MobAvoidState : MobState
{
    public MobAvoidState(AggressiveMob mob) : base(mob)
    {
        this.mob = mob;
        this.mobMovement = mob.GetComponent<MobMovement>();
    }

    protected new AggressiveMob mob;
    protected MobMovement mobMovement;
    public override bool CanAgro { get => false; }
    public override void Enter()
    {
        Debug.Log("Enter avoid state");
        this.mob.ResetAgro();
        this.mob.takeDamageProcessQuerry.Add(TakeZeroDamage);
        this.mobMovement.SetSpeed(this.mobMovement.RunSpeed);
        this.mobMovement.SetDestination(this.mob.startPosition);
        this.mob.GetComponent<Animator>().SetBool("isRunning", true);
    }

    public override void Exit()
    {
        this.mob.GetComponent<Animator>().SetBool("isRunning", false);
        this.mobMovement.ResetDestination();
        this.mob.takeDamageProcessQuerry.Remove(TakeZeroDamage);
    }

    public override MobState Update(float timeInState)
    {
        if (Vector3.Distance(this.mob.transform.position, this.mob.startPosition) < 2)
        {
            Debug.Log(Vector3.Distance(this.mob.transform.position, this.mob.startPosition));
            return new MobIdleState(this.mob);
        }
        return this;
    }

    static void TakeZeroDamage(ref DamageMessage damageMessage)
    {
        damageMessage.damage = 0;
    }
}

