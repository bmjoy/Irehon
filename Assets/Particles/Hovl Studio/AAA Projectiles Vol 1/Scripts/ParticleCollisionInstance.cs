/*This script created by using docs.unity3d.com/ScriptReference/MonoBehaviour.OnParticleCollision.html*/
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollisionInstance : MonoBehaviour
{
    public GameObject[] EffectsOnCollision;
    public float DestroyTimeDelay = 5;
    public bool UseWorldSpacePosition;
    public float Offset = 0;
    public Vector3 rotationOffset = new Vector3(0, 0, 0);
    public bool useOnlyRotationOffset = true;
    public bool UseFirePointRotation;
    public bool DestoyMainEffect = true;
    private ParticleSystem part;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    private ParticleSystem ps;

    private void Start()
    {
        this.part = this.GetComponent<ParticleSystem>();
    }

    private void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = this.part.GetCollisionEvents(other, this.collisionEvents);
        for (int i = 0; i < numCollisionEvents; i++)
        {
            foreach (GameObject effect in this.EffectsOnCollision)
            {
                GameObject instance = Instantiate(effect, this.collisionEvents[i].intersection + this.collisionEvents[i].normal * this.Offset, new Quaternion());
                if (!this.UseWorldSpacePosition)
                {
                    instance.transform.parent = this.transform;
                }

                if (this.UseFirePointRotation)
                {
                    instance.transform.LookAt(this.transform.position);
                }
                else if (this.rotationOffset != Vector3.zero && this.useOnlyRotationOffset)
                {
                    instance.transform.rotation = Quaternion.Euler(this.rotationOffset);
                }
                else
                {
                    instance.transform.LookAt(this.collisionEvents[i].intersection + this.collisionEvents[i].normal);
                    instance.transform.rotation *= Quaternion.Euler(this.rotationOffset);
                }
                Destroy(instance, this.DestroyTimeDelay);
            }
        }
        if (this.DestoyMainEffect == true)
        {
            Destroy(this.gameObject, this.DestroyTimeDelay + 0.5f);
        }
    }
}
