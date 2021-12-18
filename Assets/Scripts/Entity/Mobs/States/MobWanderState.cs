using UnityEngine;

public class MobWanderState : MobState
{
    public MobWanderState(Mob mob) : base(mob)
    {
        this.mobMovement = mob.GetComponent<MobMovement>();
    }

    protected MobMovement mobMovement;

    public override void Enter()
    {
        this.mobMovement.Wander();
        this.mob.GetComponent<Animator>().SetBool("isWalking", true);
    }

    public override void Exit()
    {
        this.mob.GetComponent<Animator>().SetBool("isWalking", false);
        this.mobMovement.ResetDestination();
    }

    public override MobState Update(float timeInState)
    {
        if (this.mobMovement.IsDestinationReached || timeInState > 10f)
        {
            return new MobIdleState(this.mob);
        }

        return this;
    }
}
