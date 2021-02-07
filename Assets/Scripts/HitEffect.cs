using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffect : MonoBehaviour
{
    [SerializeField]
    private float hitEffectDuration = 1.5f;
    private ParticleSystem particle;
    private Vector3 originalLocalPosition;
    private AudioSource releasingEffectSound;
    private Transform originalParent;

    private void Start()
    {
        particle = GetComponent<ParticleSystem>();
        releasingEffectSound = GetComponent<AudioSource>();
        if (hitEffectDuration < releasingEffectSound.clip.length + 0.01f)
            hitEffectDuration = releasingEffectSound.clip.length;
    }

    public void ReleaseEffect()
    {
        originalLocalPosition = transform.localPosition;
        originalParent = transform.parent;
        transform.SetParent(null);
        particle.Play();
        releasingEffectSound.Play();
        Invoke("EndHitEffect", hitEffectDuration);
    }

    private void EndHitEffect()
    {
        transform.parent = originalParent;
        transform.localPosition = originalLocalPosition;
    }
}
