using Irehon.Client;
using System.Collections;
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
            {
                Destroy(this.gameObject);
            }
            else
            {
                i = this;
            }

            DontDestroyOnLoad(this.gameObject);

            this.parentBG = this.textComponent.transform.parent.GetComponent<Image>();
        }

        private IEnumerator DisappearNotification()
        {
            yield return new WaitForSeconds(1.5f);
            while (this.textComponent.color.a > 0)
            {
                this.textComponent.color = new Color(1, 1, 1, this.textComponent.color.a - 0.25f);
                this.parentBG.color = new Color(1, 1, 1, this.textComponent.color.a - 0.25f);
                yield return new WaitForSeconds(.1f);
            }
            this.textComponent.text = "";
            this.parentBG.gameObject.SetActive(false);
        }

        public static void ShowMessage(ServerMessage msg)
        {
            if (msg.messageType == MessageType.Notification || msg.messageType == MessageType.Error)
            {
                i.MessageShowAndHide(msg);
            }
        }

        public static void ShowMessage(string msg)
        {
            i.MessageShowAndHide(new ServerMessage() { message = msg, messageType = MessageType.Notification });
        }

        private void MessageShowAndHide(ServerMessage msg)
        {
            if (msg.message == "")
            {
                return;
            }

            if (this.opacity != null)
            {
                this.StopCoroutine(i.opacity);
            }

            this.parentBG.gameObject.SetActive(true);
            this.textComponent.text += msg.message + System.Environment.NewLine;
            this.textComponent.color = Color.white;
            this.parentBG.color = Color.white;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)this.parentBG.transform);
            this.opacity = this.StartCoroutine(this.DisappearNotification());
        }
    }
}