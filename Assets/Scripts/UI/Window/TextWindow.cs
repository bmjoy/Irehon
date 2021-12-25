using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Irehon.UI
{
    public class TextWindow : MonoBehaviour
    {
        public static TextWindow Instance;

        [SerializeField]
        private UIWindow window;
        [SerializeField]
        private Text text;
        [SerializeField]
        private Text header;

        private void Awake()
        {
            Instance = this;
        }

        public void ShowText(string header, string message, int font, Color color)
        {
            this.header.text = header;
            text.text = message;
            text.color = color;
            text.fontSize = font;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Instance.text.transform);
            window.Open();
        }

        public void CloseTextWindow()
        {
            window.Close();
        }
    }
}