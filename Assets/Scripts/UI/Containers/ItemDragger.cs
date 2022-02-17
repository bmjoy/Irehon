using Irehon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Irehon.UI
{
    public class ItemDragger : MonoBehaviour
    {
        public static ItemDragger Instance { get; private set; }
        [SerializeField]
        private RectTransform dragger;
        [SerializeField]
        private Image draggerImage;

        [SerializeField]
        private Canvas canvas;

        private void Awake()
        {
            Instance = this;
        }

        public RectTransform GetDragger()
        {
            return dragger;
        }

        public float GetCanvasScaleFactor() => canvas.scaleFactor;

        public Image GetDraggerImage()
        {
            return draggerImage;
        }
    }
}