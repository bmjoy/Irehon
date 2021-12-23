using UnityEngine;

namespace PolyPerfect
{
    public class Common_PlaySound : MonoBehaviour
    {
        [SerializeField]
        private AudioClip animalSound;
        [SerializeField]
        private AudioClip walking;
        [SerializeField]
        private AudioClip eating;
        [SerializeField]
        private AudioClip running;
        [SerializeField]
        private AudioClip attacking;
        [SerializeField]
        private AudioClip death;
        [SerializeField]
        private AudioClip sleeping;

        private void AnimalSound()
        {
            if (this.animalSound)
            {
                Common_AudioManager.PlaySound(this.animalSound, this.transform.position);
            }
        }

        private void Walking()
        {
            if (this.walking)
            {
                Common_AudioManager.PlaySound(this.walking, this.transform.position);
            }
        }

        private void Eating()
        {
            if (this.eating)
            {
                Common_AudioManager.PlaySound(this.eating, this.transform.position);
            }
        }

        private void Running()
        {
            if (this.running)
            {
                Common_AudioManager.PlaySound(this.running, this.transform.position);
            }
        }

        private void Attacking()
        {
            if (this.attacking)
            {
                Common_AudioManager.PlaySound(this.attacking, this.transform.position);
            }
        }

        private void Death()
        {
            if (this.death)
            {
                Common_AudioManager.PlaySound(this.death, this.transform.position);
            }
        }

        private void Sleeping()
        {
            if (this.sleeping)
            {
                Common_AudioManager.PlaySound(this.sleeping, this.transform.position);
            }
        }
    }
}