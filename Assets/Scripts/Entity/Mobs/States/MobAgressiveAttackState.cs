using UnityEngine;

public class MobAgressiveAttackState : MobState
{
    public MobAgressiveAttackState(AggressiveMob mob) : base(mob)
    {
        this.mob = mob;
        this.mobMovement = mob.GetComponent<MobMovement>();

    }
    protected MobMovement mobMovement;
    protected new AggressiveMob mob;
    public override void Enter()
    {
        this.mob.GetComponent<Animator>().SetBool("isAttacking", true);
        this.mob.GetComponent<MobAnimationAttackHolder>().MeleeWeaponCollider.StartCollectColliders();

    }

    public override void Exit()
    {
        this.mob.GetComponent<Animator>().SetBool("isAttacking", false);
        this.mob.GetComponent<MobAnimationAttackHolder>().MeleeWeaponCollider.StopCollectColliders();
    }

    public override MobState Update(float timeInState)
    {
        if (this.mob.target == null)
        {
            return new MobIdleState(this.mob);
        }

        if (Vector3.Distance(this.mob.transform.position, this.mob.target.transform.position) > this.mob.RequiredAttackDistance)
        {
            return new MobAgressiveWanderState(this.mob);
        }

        this.mobMovement.SetDestination(this.mob.target.transform.position);

        return this;
    }
}
