using UnityEngine;
using UnityEngine.AI;

public class MobMovement : MonoBehaviour
{
    public float RunSpeed => this.runSpeed;
    public float WalkingSpeed => this.walkingSpeed;
    public bool IsDestinationReached => this.isDestinationReached;

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
        this.navMeshAgent = this.GetComponent<NavMeshAgent>();
        this.firstPosition = this.transform.position;
    }

    private void FixedUpdate()
    {
        if (Vector3.Distance(this.currentDestination, this.transform.position) < 1.5f)
        {
            this.isDestinationReached = true;
        }
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
        this.currentDestination = destination;
        this.navMeshAgent.SetDestination(this.currentDestination);
        this.isDestinationReached = false;
    }

    public void Wander()
    {
        this.SetDestination(RandomNavSphere(this.firstPosition, this.wanderRadius, -1));
    }

    public void ResetDestination()
    {
        this.navMeshAgent.ResetPath();
    }

    public void SetSpeed(float speed)
    {
        this.navMeshAgent.speed = speed;
    }

    private void OnDrawGizmosSelected()
    {
        if (this.firstPosition != Vector3.zero)
        {
            Gizmos.DrawWireSphere(this.firstPosition, this.wanderRadius);
        }
        else
        {
            Gizmos.DrawWireSphere(this.transform.position, this.wanderRadius);
        }
    }
}
