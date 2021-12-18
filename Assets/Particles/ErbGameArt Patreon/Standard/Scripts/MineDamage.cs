using System.Collections.Generic;
using UnityEngine;

public class MineDamage : MonoBehaviour
{
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    private ParticleSystem ps;

    private void Start()
    {
        this.ps = this.GetComponent<ParticleSystem>();
    }

    private void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = this.ps.GetCollisionEvents(other, this.collisionEvents);
        int i = 0;
        while (i < numCollisionEvents)
        {
            this.Visible();
            i++;
        }
    }

    public GameObject Object;
    private void Visible()
    {
        this.Object.SetActive(true);
    }
}
