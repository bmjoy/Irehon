using UnityEngine;

public class MobIdleState : MobState
{
    public MobIdleState(Mob mob) : base(mob)
    {
        this.wanderTimer = Random.Range(1, 7);
    }
    private float wanderTimer;
    public override void Enter()
    {
        Debug.Log("Enter idle state");
    }

    public override void Exit()
    {
    }

    public override MobState Update(float timeInState)
    {
        if (timeInState > this.wanderTimer)
        {
            return new MobWanderState(this.mob);
        }

        return this;
    }
}
