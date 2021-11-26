using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using SimpleJSON;
using DuloGames.UI;
using System;

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
            print("Client authenticate request");
            currentRequest = new AuthRequestMessage()
            {
                Id = SteamManager.GetSteamId(),
                AuthData = SteamManager.GetAuthTicket().Data
            };
            if (PlayerPrefs.GetString("Registration") != "")
            {
                currentRequest.registerInfo = new RegisterInfo(JSON.Parse(PlayerPrefs.GetString("Registration")));
            }
            NetworkClient.Send(currentRequest);
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
            try
            {
                SteamManager.StartClient();
            }
            catch (Exception exception)
            {
                ServerMessageNotificator.ShowMessage($"Client intialize steam error: {exception.Message}");
                return;
            }
            try 
            {
                GetComponent<NetworkManager>().StartClient();
            }
            catch (Exception exception)
            {
                ServerMessageNotificator.ShowMessage($"Client play error: {exception.Message}");
                return;
            }
        }

        private void OnAuthResponseMessage(ServerMessage msg)
        {
            if (msg.messageType == MessageType.AuthAccept)
            {
                LoginSceneUI.HidePlayButton();
                ClientAccept();
            }
            else if (msg.messageType == MessageType.AuthReject)
                ClientReject();
        }
    }
}