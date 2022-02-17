using UnityEngine;

public class MobAgressiveWanderState : MobState
{
    public MobAgressiveWanderState(AggressiveMob mob) : base(mob)
    {
        this.mob = mob;
        this.mobMovement = mob.GetComponent<MobMovement>();
    }
    protected new AggressiveMob mob;

    protected MobMovement mobMovement;
    public override void Enter()
    {
        this.mob.GetComponent<Animator>().SetBool("isRunning", true);
        this.mobMovement.SetSpeed(this.mobMovement.RunSpeed);
    }
    public override void Exit()
    {
        this.mob.GetComponent<Animator>().SetBool("isRunning", false);
        this.mobMovement.SetSpeed(this.mobMovement.WalkingSpeed);
        this.mobMovement.ResetDestination();
    }

    public override MobState Update(float timeInState)
    {
        //Is destroyed
        if (this.mob.target == null)
        {
            return new MobIdleState(this.mob);
        }

        //Is runned away
        if (timeInState > 15 && Vector3.Distance(this.mob.transform.position, this.mob.target.transform.position) > this.mob.UnagroRadius)
        {
            this.mob.UnAgro();
            return new MobIdleState(this.mob);
        }

        if (Vector3.Distance(this.mob.transform.position, this.mob.startPosition) > this.mob.AvoidRadius)
        {
            this.mob.SetDefaultState();
            return new MobIdleState(this.mob);
        }
        if (Vector3.Distance(this.mob.transform.position, this.mob.target.transform.position) < this.mob.RequiredAttackDistance)
        {
            return new MobAgressiveAttackState(this.mob);
        }

        this.mobMovement.SetDestination(this.mob.target.transform.position);

        return this;
    }
}
