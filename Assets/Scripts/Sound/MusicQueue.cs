using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon.Sound
{
    public class MusicQueue
    {
        private List<AudioClip> clips;

        public MusicQueue(List<AudioClip> clips)
        {
            this.clips = clips;
        }

        public IEnumerator LoopMusic(MonoBehaviour player, float delay, System.Action<AudioClip> playFunction)
        {
            while (true)
            {
                yield return player.StartCoroutine(this.Run(this.RandomizeList(this.clips), delay, playFunction));
            }
        }

        public IEnumerator Run(List<AudioClip> tracks,
            float delay, System.Action<AudioClip> playFunction)
        {
            foreach (AudioClip clip in tracks)
            {
                playFunction(clip);

                yield return new WaitForSeconds(clip.length + delay);
            }
        }

        public List<AudioClip> RandomizeList(List<AudioClip> list)
        {
            List<AudioClip> copy = new List<AudioClip>(list);

            int n = copy.Count;

            while (n > 1)
            {
                n--;

                int k = Random.Range(0, n + 1);

                AudioClip value = copy[k];

                copy[k] = copy[n];
                copy[n] = value;
            }

            return copy;
        }
    }
}