using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

public class MobMovement : MonoBehaviour
{
    public float RunSpeed => runSpeed;
    public float WalkingSpeed => walkingSpeed;
    public bool IsDestinationReached => isDestinationReached;

    [SerializeField]
    private float wanderRadius;
    [SerializeField]
    private float runSpeed = 5f;
    [SerializeField]
    private float walkingSpeed = 3.5f;

    private Vector3 currentDestination;
    private Vector3 firstPosition;
    private NavMeshAgent navMeshAgent;

    private bool isDestinationReached;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        firstPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (Vector3.Distance(currentDestination, transform.position) < 1.5f)
            isDestinationReached = true;
    }

    private static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;

        randDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        return navHit.position;
    }

    public void SetDestination(Vector3 destination)
    {
        currentDestination = destination;
        navMeshAgent.SetDestination(currentDestination);
        isDestinationReached = false;
    }

    public void Wander()
    {
        SetDestination(RandomNavSphere(firstPosition, wanderRadius, -1));
    }

    public void ResetDestination()
    {
        navMeshAgent.ResetPath();
    }
    
    public void SetSpeed(float speed)
    {
        navMeshAgent.speed = speed;
    }

    private void OnDrawGizmosSelected()
    {
        if (firstPosition != Vector3.zero)
            Gizmos.DrawWireSphere(firstPosition, wanderRadius);
        else
            Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}
