using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Irehon.Chat
{
    public class ChatWindow : MonoBehaviour
    {
        public static ChatWindow Instance;
        public delegate void NewMessageEventHandler(string message);
        public delegate void ChatOpenedEventHandler(bool isChatOpened);

        public event NewMessageEventHandler MessageSended;
        public event ChatOpenedEventHandler ChatActiveStateChanged;

        public bool isChatOpened;
        
        [SerializeField]
        private GameObject messagesContentHolder;
        [SerializeField]
        private GameObject chatTabPrefab;
        [SerializeField]
        private InputField chatInputField;
        [SerializeField]
        private Scrollbar scrollbar;
        [SerializeField]
        private Image background;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }

            ChatActiveStateChanged += 
                isOpened => scrollbar.gameObject.GetComponent<CanvasGroup>().alpha = isOpened ? 1 : 0;

            ChatActiveStateChanged +=
                isOpened => background.enabled = isOpened;
        }

        private void Update()
        {
            if (GameSession.IsListeningGameKeys && Input.GetKeyDown(KeyCode.Return))
            {
                print("pressed");
                chatInputField.interactable = true;
                this.chatInputField.ActivateInputField();
            }

            if (this.chatInputField.isFocused)
            {
                chatInputField.interactable = true;
                this.isChatOpened = true;
                ChatActiveStateChanged?.Invoke(isChatOpened);
                GameSession.IsListeningGameKeys = false;
            }
            if (this.isChatOpened && !this.chatInputField.isFocused)
            {
                this.isChatOpened = false;
                ChatActiveStateChanged?.Invoke(isChatOpened);
                GameSession.IsListeningGameKeys = true;
            }
        }

        public void ShowMessage(ulong steamId, string message)
        {
            GameObject newMessage = Instantiate(this.chatTabPrefab, this.messagesContentHolder.transform);
            newMessage.GetComponent<Message>().Intiallize(steamId, message);
        }

        public void ChatMessage()
        {
            chatInputField.DeactivateInputField();
            chatInputField.interactable = false;
            if (this.chatInputField.text == "")
            {
                return;
            }

            MessageSended.Invoke(chatInputField.text);
            this.chatInputField.text = "";
        }
    }
}