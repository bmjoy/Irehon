using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Client
{
    public class ServerMessageNotificator : MonoBehaviour
    {
        [SerializeField]
        private Text textComponent;

        private static ServerMessageNotificator i;
        private Image parentBG;
        private Coroutine opacity;

        private void Awake()
        {
            if (i != null && i != this)
                Destroy(gameObject);
            else
                i = this;
            DontDestroyOnLoad(gameObject);

            parentBG = textComponent.transform.parent.GetComponent<Image>();
        }

        private IEnumerator DisappearNotification()
        {
            yield return new WaitForSeconds(1.5f);
            while (textComponent.color.a > 0)
            {
                textComponent.color = new Color(1, 1, 1, textComponent.color.a - 0.25f);
                parentBG.color = new Color(1, 1, 1, textComponent.color.a - 0.25f);
                yield return new WaitForSeconds(.1f);
            }
            textComponent.text = "";
            parentBG.gameObject.SetActive(false);
        }

        public static void ShowMessage(ServerMessage msg)
        {
            if (msg.messageType == MessageType.Notification || msg.messageType == MessageType.Error)
                i.MessageShowAndHide(msg);
        }

        private void MessageShowAndHide(ServerMessage msg)
        {
            if (msg.message == "")
                return;
            if (opacity != null)
                StopCoroutine(i.opacity);
            parentBG.gameObject.SetActive(true);
            textComponent.text += msg.message + System.Environment.NewLine;
            textComponent.color = Color.white;
            parentBG.color = Color.white;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)parentBG.transform);
            opacity = StartCoroutine(DisappearNotification());
        }
    }
}