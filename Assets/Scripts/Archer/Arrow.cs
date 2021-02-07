using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private const float TIME_TO_DESPAWN = 4f;
    [SerializeField]
    private int hitDamage;
    [SerializeField]
    private HitEffect hitEffect;
    public Rigidbody rigidBody;
    private float power;
    private bool flying = true;
    private float time = 0f;
    [SerializeField]
    private SniperArrowParticle particle;
    private Player arrowOwner;
    private Bow.Quiver quiver;
    private List<Collider> selfColliders;

    public void ResetArrow()
    {
        time = 0;
        flying = true;
        rigidBody.useGravity = true;
    }

    private int GetDamage()
    {
        return Convert.ToInt32(hitDamage * .3f + hitDamage * power);
    }

    void Update()
    {
        time += Time.deltaTime;
        if (flying && rigidBody.velocity != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(rigidBody.velocity);
        if (time > TIME_TO_DESPAWN)
        {
            quiver.ReturnArrowInQuiver(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || !flying)
            return;
        if (selfColliders.Contains(other))
            return;
        flying = false;
        rigidBody.useGravity = false;
        rigidBody.velocity = Vector3.zero;
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

    public void SetParent(Player arrowOwner, List<Collider> selfColliders, Bow.Quiver quiver)
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
