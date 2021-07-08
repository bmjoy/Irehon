using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    protected const float TIME_TO_DESPAWN = 4f;
    [SerializeField]
    protected int hitDamage;
    [SerializeField]
    protected HitEffect hitEffect;
    [SerializeField]
    protected ParticleSystem releaseEffect;
    public Rigidbody rigidBody;
    [SerializeField]
    protected AudioSource releaseSound;
    protected float power;
    protected bool flying = true;
    protected float time = 0f;
    [SerializeField]
    protected SniperArrowParticle particle;
    protected Player arrowOwner;
    protected Quiver quiver;
    protected List<Collider> selfColliders;

    public void ResetArrow()
    {
        time = 0;
        flying = true;
        rigidBody.useGravity = true;
    }

    public void TriggerReleaseEffect()
    {
        if (releaseSound != null)
            releaseSound.Play();
        if (releaseEffect != null)
            releaseEffect.Play();
    }

    protected int GetDamage()
    {
        return Convert.ToInt32(hitDamage * .3f + hitDamage * power);
    }

    protected void Update()
    {
        time += Time.deltaTime;
        if (flying && rigidBody.velocity != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(rigidBody.velocity);
        if (time > TIME_TO_DESPAWN)
        {
            quiver.ReturnArrowInQuiver(this);
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EntityBase") || other.CompareTag("Ability") || !flying)
            return;
        if (selfColliders.Contains(other))
            return;
        flying = false;
        rigidBody.useGravity = false;
        rigidBody.velocity = Vector3.zero;
        if (hitEffect != null)
            hitEffect.ReleaseEffect();
        HittedColliderProcess(other);
        if (other.CompareTag("Entity"))
            quiver.ReturnArrowInQuiver(this);
    }

    public void HittedColliderProcess(Collider collider)
    {
        if (collider.CompareTag("Entity"))
        {
            arrowOwner.DoDamage(collider.GetComponent<EntityCollider>().GetParentEntityComponent(), GetDamage());
        }
    }

    public void SetParent(Player arrowOwner, List<Collider> selfColliders, Quiver quiver)
    {
        this.arrowOwner = arrowOwner;
        this.selfColliders = selfColliders;
        this.quiver = quiver;
    }

    public void SetPower(float power)
    {
        this.power = power;
        if (particle != null)
            particle.SetWaveSize(power);
    }
}
