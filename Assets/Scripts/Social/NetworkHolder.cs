using Irehon.Client;
using Mirror;
using UnityEngine;

namespace Irehon.Chat
{
    public class NetworkHolder : NetworkBehaviour
    {
        private float lastMessageTimer;

        private void Start()
        {
            if (this.isLocalPlayer)
            {
                UIChatEventHolder.Instance.NewMessageSended += SendMessageToServer;
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
                Irehon.ServerManager.SendMessage(this.connectionToClient, "Don't spam messages", MessageType.Notification);
                return;
            }
            this.lastMessageTimer = 0.75f;
            Irehon.ServerManager.Log(this.GetComponent<Player>().Id, $"Chat message: {message}");
            this.RecieveChatMessageRpc(message);
        }

        [ClientRpc]
        private void RecieveChatMessageRpc(string message)
        {
            UIChatEventHolder.Instance.ShowMessage(this.GetComponent<Player>().Id, message);
        }
    }
}