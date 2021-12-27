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
            
        }

        [ClientRpc]
        private void RecieveChatMessageRpc(string message)
        {
            ChatWindow.Instance.ShowMessage(this.GetComponent<Player>().Id, message);
        }
    }
}