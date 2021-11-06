using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using SimpleJSON;
using DuloGames.UI;

namespace Client
{
    public class ClientAuth : NetworkAuthenticator
    {
        private void Start()
        {
            
        }
        public override void OnClientAuthenticate()
        {
            currentRequest = new AuthRequestMessage()
            {
                Id = SteamManager.GetSteamId(),
                AuthData = SteamManager.GetAuthTicket().Data
            };
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
            if (PlayerPrefs.GetString("Registration") != "")
            {
                currentRequest.registerInfo = new RegisterInfo(JSON.Parse(PlayerPrefs.GetString("Registration")));
            }
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