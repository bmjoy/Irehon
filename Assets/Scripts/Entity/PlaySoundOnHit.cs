using UnityEngine;

public class PlaySoundOnHit : MonoBehaviour
{
    public AudioClip clip;

    private void Start()
    {
        this.GetComponent<AudioSource>().playOnAwake = false;
        this.GetComponent<AudioSource>().clip = this.clip;
    }

    private void OnTrigger()
    {
        this.GetComponent<AudioSource>().Play();
    }
}
