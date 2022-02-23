using Irehon;
using Irehon.Entitys;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Irehon.UI
{
    public class Hint : MonoBehaviour
    {
        public static Hint Instance;

        [SerializeField]
        private Image cursorHint;
        [SerializeField]
        private GameObject interactableHint;

        [SerializeField]
        private Text header;
        [SerializeField]
        private Text content;


        private void Awake() => Instance = this;

        private void Start()
        {
            Mouse.CursorChanged += this.ChangeCursorHintStatus;
        }

        public void ShowHint(string header, string content)
        {
            this.interactableHint?.SetActive(true);
            this.header.text = header;
            this.content.text = content;
        }

        public void HideHint()
        {
            this.interactableHint?.SetActive(false);
        }

        private void ChangeCursorHintStatus(bool isCursorEnabled)
        {
            float alpha = isCursorEnabled ? 0.9f : 0.3f;

            Color color = Color.white;
            color.a = alpha;
            this.cursorHint.color = color;
        }

        private void OnDestroy()
        {
            Mouse.CursorChanged -= this.ChangeCursorHintStatus;
        }
    }
}