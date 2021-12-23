using System.Collections;
using UnityEngine;

namespace Irehon.UI
{
    public class Canvases : MonoBehaviour
    {
        public static Canvases Instance { get; private set; }

        [SerializeField]
        private Canvas statusBarCanvas;

        private void Awake() => Instance = this;

        public void ShowStatusCanvas()
        {
            this.statusBarCanvas.gameObject.SetActive(true);
        }

        public void HideStatusCanvas()
        {
            this.statusBarCanvas.gameObject.SetActive(false);
        }
    }
}