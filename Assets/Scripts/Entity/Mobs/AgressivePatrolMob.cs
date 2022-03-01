using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgressivePatrolMob : AggressiveMob
{
    public Vector3[] pointsToMove;
    public int countOfMoves;
    // Start is called before the first frame update
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
