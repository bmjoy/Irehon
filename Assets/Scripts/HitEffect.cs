using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffect : MonoBehaviour
{
    [SerializeField]
    private float hitEffectDuration = 1.5f;
    private ParticleSystem particle;
    private Vector3 originalLocalPosition;
    private Transform originalParent;

    private void Start()
    {
        particle = GetComponent<ParticleSystem>();
    }

    public void ReleaseEffect()
    {
        originalLocalPosition = transform.localPosition;
        originalParent = transform.parent;
        transform.SetParent(null);
        particle.Play();
        Invoke("EndHitEffect", hitEffectDuration);
    }

    private void EndHitEffect()
    {
        transform.parent = originalParent;
        transform.localPosition = originalLocalPosition;
    }
}
