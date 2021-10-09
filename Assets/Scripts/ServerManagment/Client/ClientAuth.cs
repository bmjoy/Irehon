using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

namespace Client
{
    public class ClientAuth : NetworkAuthenticator
    {
        private void Start()
        {
            
        }
        public override void OnClientAuthenticate()
        {
            NetworkClient.Send(currentRequest);
        }

        public override void OnServerAuthenticate(NetworkConnection conn)
        {
        }

        private AuthRequestMessage currentRequest;

        public override void OnStartClient()
        {
            ClientManager.OnGetServerMessage.AddListener(OnAuthResponseMessage);
        }

        public void PlayButton()
        {
            currentRequest = new AuthRequestMessage()
            {
                Id = SteamManager.GetSteamId(),
                AuthData = SteamManager.GetAuthTicket().Data
            };

            GetComponent<NetworkManager>().StartClient();
        }

        private void OnAuthResponseMessage(ServerMessage msg)
        {
            if (msg.messageType == MessageType.AuthAccept)
                ClientAccept();
            else if (msg.messageType == MessageType.AuthReject)
                ClientReject();
        }
    }
}