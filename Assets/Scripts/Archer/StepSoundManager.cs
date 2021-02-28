using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepSoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] stepPool;

    [SerializeField]
    private AudioSource stepSource;

    public void Step()
    {
        stepSource.clip = stepPool[Random.Range(0, stepPool.Length)];
        stepSource.Play();
    }
}
