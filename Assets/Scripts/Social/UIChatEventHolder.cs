using UnityEngine;
using UnityEngine.Events;

namespace Irehon.Chat
{
    public class UIChatEventHolder : MonoBehaviour
    {
        public delegate void NewMessageEventHandler(string message);
        public static UIChatEventHolder Instance;

        public event NewMessageEventHandler NewMessageSended;

        [SerializeField]
        private GameObject messagesContentHolder;
        [SerializeField]
        private GameObject chatTabPrefab;

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
        }

        public void SendNewMessage(string message) => NewMessageSended.Invoke(message);


        public void ShowMessage(ulong steamId, string message)
        {
            GameObject newMessage = Instantiate(this.chatTabPrefab, this.messagesContentHolder.transform);
            newMessage.GetComponent<Message>().Intiallize(steamId, message);
        }
    }
}