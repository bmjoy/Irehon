using Irehon.Entitys;
using UnityEngine;
using UnityEngine.Events;

public class AgressivePatrolMob : AggressiveMob
{
    public Vector3[] pointsToMove;
    public int countOfMoves;

    protected override void Start()
    {
        base.Start();
        this.stateMachine.SetNewState(new MobPatrolState(this));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pointsToMove[countOfMoves], 0.2f);
        Gizmos.DrawSphere(this.startPosition, 0.2f);
    }

}
