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
        public static bool isShouldAutoLoad = false;
        private AuthInfo currentRequest = new AuthInfo();

        public static RegisterInfo RegisterInfo = new RegisterInfo(Entitys.Fraction.None);

        private void Start()
        {
            if (isShouldAutoLoad)
            {
                PlayButton();
                isShouldAutoLoad = false;
            }
        }
        public override void OnClientAuthenticate()
        {
            print("Client authenticate request");
            this.currentRequest = new AuthInfo()
            {
                version = Application.version,
                Id = SteamManager.GetSteamId(),
                AuthData = SteamManager.GetAuthTicket().Data
            };
            if (RegisterInfo.fraction != Entitys.Fraction.None)
            {
                this.currentRequest.registerInfo = RegisterInfo;
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
                print("Started client");
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
                Debug.Log("Accepted");
                LoginSceneUI.HidePlayButton();
                LoginSceneUI.ShowLoadingBar();
                this.ClientAccept();
            }
            else if (msg.messageType == MessageType.AuthReject)
            {
                if (!NetworkClient.isConnected)
                    return;
                ServerMessageNotificator.ShowMessage($"Client authentication error: {msg.message}");
                Debug.Log("Rejected");
                NetworkClient.Disconnect();
                //this.ClientReject();
            }
        }
    }
}