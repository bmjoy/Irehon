using UnityEngine;
using UnityEngine.Events;

namespace Irehon.Chat
{
    public class UIChatEventHolder : MonoBehaviour
    {
        public class PlayerChatInputEventHandler : UnityEvent<string> { }
        public static UIChatEventHolder instance;

        public PlayerChatInputEventHandler PlayerChatInputEvent { get; private set; } = new PlayerChatInputEventHandler();

        [SerializeField]
        private GameObject messagesContentHolder;
        [SerializeField]
        private GameObject chatTabPrefab;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                instance = this;
            }
        }

        public void ShowMessage(ulong steamId, string message)
        {
            GameObject newMessage = Instantiate(this.chatTabPrefab, this.messagesContentHolder.transform);
            newMessage.GetComponent<Message>().Intiallize(steamId, message);
        }
    }
}