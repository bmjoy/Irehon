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
    [SerializeField]
    private Rigidbody rigidBody;
    private float power;
    private bool flying = true;
    private float time = 0f;
    private Bow parentBow;
    private List<Collider> selfColliders;

    public void ResetArrow()
    {
        time = 0;
        flying = true;
        rigidBody.useGravity = true;
    }

    public int GetDamage()
    {
        return Convert.ToInt32(hitDamage * 0.5f + hitDamage * power);
    }

    void Update()
    {
        time += Time.deltaTime;
        if (flying && rigidBody.velocity != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(rigidBody.velocity);
        if (time > TIME_TO_DESPAWN)
        {
            parentBow.ReturnArrowInQuiver(this);
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
        parentBow.HittedColliderProcess(other, this);
        if (other.CompareTag("Entity"))
            parentBow.ReturnArrowInQuiver(this);
    }

    public void SetParentBow(Bow parent)
    {
        parentBow = parent;
    }

    public void SetPower(float power)
    {
        this.power = power;
    }

    public void SetRegisteredColliders(List<Collider> selfColliders)
    {
        this.selfColliders = selfColliders;
    }
}
