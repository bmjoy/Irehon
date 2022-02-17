using UnityEngine;

public class MobFearState : MobState
{
    public MobFearState(Mob mob) : base(mob)
    {
        this.mobMovement = mob.GetComponent<MobMovement>();
    }

    protected MobMovement mobMovement;

    public override void Enter()
    {
        this.mobMovement.SetSpeed(this.mobMovement.RunSpeed);
        this.mobMovement.Wander();
        this.mob.GetComponent<Animator>().SetBool("isRunning", true);
    }

    public override void Exit()
    {
        this.mobMovement.SetSpeed(this.mobMovement.WalkingSpeed);
        this.mob.GetComponent<Animator>().SetBool("isRunning", false);
        this.mobMovement.ResetDestination();
    }

    public override MobState Update(float timeInState)
    {
        if (timeInState > 40f)
        {
            return new MobIdleState(this.mob);
        }

        if (this.mobMovement.IsDestinationReached)
        {
            this.mobMovement.Wander();
        }

        return this;
    }
}
