using Irehon.Client;
using Mirror;
using UnityEngine;

namespace Irehon.Chat
{
    public class NetworkChatHolder : NetworkBehaviour
    {
        private float lastMessageTimer;

        private void Start()
        {
            if (this.isLocalPlayer)
            {
                ChatWindow.Instance.MessageSended += SendMessageToServer;
            }
        }

        private void FixedUpdate()
        {
            if (this.lastMessageTimer > 0)
            {
                this.lastMessageTimer -= Time.fixedDeltaTime;
            }
        }

        private void SendMessageToServer(string message)
        {
            SendChatMessage(message);
        }

        [Command]
        public void SendChatMessage(string message)
        {
            if (this.lastMessageTimer > 0)
            {
                ServerManager.SendMessage(this.connectionToClient, "Don't spam messages", MessageType.Notification);
                return;
            }
            this.lastMessageTimer = 0.75f;
            ServerManager.Log(this.GetComponent<Player>().Id, $"Chat message: {message}");
            this.RecieveChatMessageRpc(message);
        }

        [ClientRpc]
        private void RecieveChatMessageRpc(string message)
        {
            ChatWindow.Instance.ShowMessage(this.GetComponent<Player>().Id, message);
        }
    }
}