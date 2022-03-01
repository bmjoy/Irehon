using UnityEngine;

public class MobPatrolState : MobState
{
	public MobPatrolState(AgressivePatrolMob mob) : base(mob)
	{
		this.mob = mob;
		this.mobMovement = mob.GetComponent<MobMovement>();
	}

	protected new AgressivePatrolMob mob;
	protected MobMovement mobMovement;
	private bool reachTheEnd;

	public override void Enter()
	{
		this.mob.GetComponent<Animator>().SetBool("isWalking", true);
		this.mobMovement.SetSpeed(this.mobMovement.WalkingSpeed);
		this.mobMovement.SetDestination(mob.pointsToMove[mob.countOfMoves]);
	}

	public override void Exit()
	{
		this.mob.GetComponent<Animator>().SetBool("isWalking", false);
		this.mobMovement.ResetDestination();
	}

	private MobState LoopPatrol()
	{
		if (mob.countOfMoves >= mob.pointsToMove.Length - 1)
		{
			Debug.Log(Vector3.Distance(this.mob.transform.position, mob.startPosition));
			if(Vector3.Distance(this.mob.transform.position, mob.pointsToMove[mob.countOfMoves]) < 1)
			{
				mob.countOfMoves = 0;
				mob.SetDefaultState();
				return this;
			}
		}
		return this;
	}

	public override MobState Update(float timeInState)
	{
		Debug.Log(mob.countOfMoves);
		if (Vector3.Distance(this.mob.transform.position, mob.pointsToMove[mob.countOfMoves]) < 3 && (mob.countOfMoves < mob.pointsToMove.Length - 1))
		{
			mob.countOfMoves++;
			this.mobMovement.SetDestination(mob.pointsToMove[mob.countOfMoves]);
		}
		return LoopPatrol();
	}
}
