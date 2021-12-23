using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Irehon.UI
{
    public class TooltipWindow : MonoBehaviour
    {
        private static TooltipWindow Instance;

        [SerializeField]
        private RectTransform customTextWindow;
        [SerializeField]
        private RectTransform spawningTextWindow;

        [SerializeField]
        private GameObject textPrefab;

        private bool isCursorFollowing = false;

        private void Awake() => Instance = this;

        private void Update()
        {
            if (this.isCursorFollowing)
            {
                Instance.customTextWindow.position = Input.mousePosition;
            }
        }

        public static void HideTooltip()
        {
            if (Instance == null)
            {
                return;
            }

            Instance.isCursorFollowing = false;
            Instance.customTextWindow.gameObject.SetActive(false);
        }

        public static void ShowTooltip(List<TooltipMessage> messages)
        {
            foreach (RectTransform previousText in Instance.spawningTextWindow)
            {
                Destroy(previousText.gameObject);
            }

            foreach (TooltipMessage message in messages)
            {
                GameObject currentText = Instantiate(Instance.textPrefab, Instance.spawningTextWindow);
                TMPro.TMP_Text textComponent = currentText.GetComponent<TMPro.TMP_Text>();
                textComponent.text = message.Message;
                textComponent.color = message.Color;
                textComponent.fontSize = message.Font;
                Canvas.ForceUpdateCanvases();
            }
            Instance.spawningTextWindow.gameObject.SetActive(true);
            Instance.customTextWindow.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Instance.spawningTextWindow.transform);
            Instance.isCursorFollowing = true;

        }
    }
}