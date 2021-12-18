using UnityEngine;

public class TargetProjectile : MonoBehaviour
{
    public float speed = 15f;
    public GameObject hit;
    public GameObject flash;
    public GameObject[] Detached;
    public bool LocalRotation = false;
    private Transform target;
    private Vector3 targetOffset;

    [Space]
    [Header("PROJECTILE PATH")]
    private float randomUpAngle;
    private float randomSideAngle;
    public float sideAngle = 25;
    public float upAngle = 20;

    private void Start()
    {
        this.FlashEffect();
        this.newRandom();
    }

    private void newRandom()
    {
        this.randomUpAngle = Random.Range(0, this.upAngle);
        this.randomSideAngle = Random.Range(-this.sideAngle, this.sideAngle);
    }

    //Link from movement controller
    //TARGET POSITION + TARGET OFFSET
    public void UpdateTarget(Transform targetPosition, Vector3 Offset)
    {
        this.target = targetPosition;
        this.targetOffset = Offset;
    }

    private void Update()
    {
        if (this.target == null)
        {
            foreach (GameObject detachedPrefab in this.Detached)
            {
                if (detachedPrefab != null)
                {
                    detachedPrefab.transform.parent = null;
                }
            }
            Destroy(this.gameObject);
            return;
        }

        Vector3 forward = ((this.target.position + this.targetOffset) - this.transform.position);
        Vector3 crossDirection = Vector3.Cross(forward, Vector3.up);
        Quaternion randomDeltaRotation = Quaternion.Euler(0, this.randomSideAngle, 0) * Quaternion.AngleAxis(this.randomUpAngle, crossDirection);
        Vector3 direction = randomDeltaRotation * ((this.target.position + this.targetOffset) - this.transform.position);

        float distanceThisFrame = Time.deltaTime * this.speed;

        if (direction.magnitude <= distanceThisFrame)
        {
            this.HitTarget();
            return;
        }

        this.transform.Translate(direction.normalized * distanceThisFrame, Space.World);
        this.transform.rotation = Quaternion.LookRotation(direction);
    }

    private void FlashEffect()
    {
        if (this.flash != null)
        {
            GameObject flashInstance = Instantiate(this.flash, this.transform.position, Quaternion.identity);
            flashInstance.transform.forward = this.gameObject.transform.forward;
            ParticleSystem flashPs = flashInstance.GetComponent<ParticleSystem>();
            if (flashPs != null)
            {
                Destroy(flashInstance, flashPs.main.duration);
            }
            else
            {
                ParticleSystem flashPsParts = flashInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(flashInstance, flashPsParts.main.duration);
            }
        }
    }

    private void HitTarget()
    {
        if (this.hit != null)
        {
            Quaternion hitRotation = this.transform.rotation;
            if (this.LocalRotation == true)
            {
                hitRotation = Quaternion.Euler(0, 0, 0);
            }
            GameObject hitInstance = Instantiate(this.hit, this.target.position + this.targetOffset, hitRotation);
            ParticleSystem hitPs = hitInstance.GetComponent<ParticleSystem>();
            if (hitPs != null)
            {
                Destroy(hitInstance, hitPs.main.duration);
            }
            else
            {
                ParticleSystem hitPsParts = hitInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(hitInstance, hitPsParts.main.duration);
            }
        }
        foreach (GameObject detachedPrefab in this.Detached)
        {
            if (detachedPrefab != null)
            {
                detachedPrefab.transform.parent = null;
            }
        }
        Destroy(this.gameObject);
    }
}
