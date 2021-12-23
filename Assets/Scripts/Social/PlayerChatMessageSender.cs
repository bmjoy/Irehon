using UnityEngine;
using UnityEngine.UI;

namespace Irehon.Chat
{
    public class PlayerChatMessageSender : MonoBehaviour
    {
        [SerializeField]
        private InputField chatInputField;


        private bool isBeenFocused;

        private void Update()
        {
            if (GameSession.IsListeningGameKeys && Input.GetKeyDown(KeyCode.Return))
            {
                this.chatInputField.ActivateInputField();
            }

            if (this.chatInputField.isFocused)
            {
                this.isBeenFocused = true;
                GameSession.IsListeningGameKeys = false;
            }
            if (this.isBeenFocused && !this.chatInputField.isFocused)
            {
                this.isBeenFocused = false;
                GameSession.IsListeningGameKeys = true;
            }
        }

        public void ChatMessage()
        {
            this.chatInputField.DeactivateInputField();
            if (this.chatInputField.text == "")
            {
                return;
            }

            UIChatEventHolder.Instance.SendNewMessage(chatInputField.text);
            this.chatInputField.text = "";
        }
    }
}