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
        private AuthRequestMessage currentRequest = new AuthRequestMessage();
        private void Start()
        {
            
        }
        public override void OnClientAuthenticate()
        {
#if !UNITY_EDITOR
            currentRequest = new AuthRequestMessage()
            {
                Id = SteamManager.GetSteamId(),
                AuthData = SteamManager.GetAuthTicket().Data
            };
#else
            if (PlayerPrefs.GetString("Registration") != "")
            {
                currentRequest.registerInfo = new RegisterInfo(JSON.Parse(PlayerPrefs.GetString("Registration")));
            }
            NetworkClient.Send(currentRequest);
#endif
        }

        public override void OnServerAuthenticate(NetworkConnection conn)
        {
        }
        public override void OnStartClient()
        {
            ClientManager.OnGetServerMessage.AddListener(OnAuthResponseMessage);
        }

        public void PlayButton()
        {
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