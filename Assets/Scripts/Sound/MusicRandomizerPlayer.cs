using System.Collections.Generic;
using UnityEngine;

namespace Irehon.Sound
{
    public class MusicRandomizerPlayer : MonoBehaviour
    {
        public List<AudioClip> musicClips = new List<AudioClip>();

        public MusicQueue musicQueue;

        public static AudioSource music;

        private AudioClip currentTrack;

        private float length;

        private Coroutine musicLoop;

        private AudioSource musicSource;

        private void Start()
        {
            this.musicQueue = new MusicQueue(this.musicClips);

            this.musicSource = this.GetComponent<AudioSource>();

            this.StartMusic();
        }

        public void PlayMusicClip(AudioClip music)
        {
            this.musicSource.Stop();
            this.musicSource.clip = music;
            this.musicSource.Play();
        }

        public void StopMusic()
        {
            if (this.musicLoop != null)
            {
                this.StopCoroutine(this.musicLoop);
            }

            music.Stop();
        }

        public void StartMusic()
        {
            this.musicLoop = this.StartCoroutine(this.musicQueue.LoopMusic(this, 0, this.PlayMusicClip));
        }
    }
}