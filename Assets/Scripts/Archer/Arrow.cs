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
    private bool flying = true;
    private float time = 0f;
    private Bow parentBow;

    public void ResetTime()
    {
        time = 0;
    }

    public int GetDamage()
    {
        return hitDamage;
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
        flying = false;
        rigidBody.useGravity = false;
        rigidBody.velocity = Vector3.zero;
        hitEffect.ReleaseEffect();
        parentBow.HittedColliderProcess(other, this);
    }

    public void SetParentBow(Bow parent)
    {
        parentBow = parent;
    }
}
