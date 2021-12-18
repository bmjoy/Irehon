using System.Collections;
using UnityEngine;

namespace Irehon
{
    public class Hitmarker : MonoBehaviour
    {
        public static Hitmarker Instance { get; private set; }
        private void Awake() => Instance = this;

        private Coroutine hitMarkerCoroutine;
        [SerializeField]
        private CanvasGroup hitMarker;
        [SerializeField]
        private AudioSource hitMarkerSound;

        public void ShowHitMarker()
        {
            if (this.hitMarkerCoroutine != null)
            {
                this.StopCoroutine(this.hitMarkerCoroutine);
            }

            this.hitMarker.alpha = 1;
            this.hitMarkerCoroutine = this.StartCoroutine(this.DisappearHitMarker());
            this.hitMarkerSound.Play();
        }

        private IEnumerator DisappearHitMarker()
        {
            while (this.hitMarker.alpha > 0)
            {
                this.hitMarker.alpha -= .1f;
                yield return new WaitForSeconds(.1f);
            }
        }
    }
}