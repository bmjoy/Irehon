using Client;
using Irehon.Steam;
using Mirror;
using SimpleJSON;
using System;
using UnityEngine;

namespace Irehon.Client
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
            this.currentRequest = new AuthRequestMessage()
            {
                Id = SteamManager.GetSteamId(),
                AuthData = SteamManager.GetAuthTicket().Data
            };
            if (PlayerPrefs.GetString("Registration") != "")
            {
                this.currentRequest.registerInfo = new RegisterInfo(JSON.Parse(PlayerPrefs.GetString("Registration")));
            }
            NetworkClient.Send(this.currentRequest);
        }

        public override void OnServerAuthenticate(NetworkConnection conn)
        {
        }
        public override void OnStartClient()
        {
            ClientManager.OnGetServerMessage.AddListener(this.OnAuthResponseMessage);
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
                this.GetComponent<NetworkManager>().StartClient();
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
                LoginSceneUI.ShowLoadingBar();
                this.ClientAccept();
            }
            else if (msg.messageType == MessageType.AuthReject)
            {
                this.ClientReject();
            }
        }
    }
}