using UnityEngine;

public class StepSoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] stepPool;

    [SerializeField]
    private AudioSource stepSource;

    public void Step()
    {
        this.stepSource.clip = this.stepPool[Random.Range(0, this.stepPool.Length)];
        this.stepSource.Play();
    }

    public void FootR()
    {
        this.Step();
    }

    public void FootL()
    {
        this.Step();
    }
}
