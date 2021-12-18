using Client;
using DuloGames.UI;
using Irehon.UI;
using Irehon.Utils;
using Mirror;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Irehon.Client
{
    public enum MessageType { AuthAccept, AuthReject, Error, Notification, RegistrationRequired, ServerRedirect, ItemDatabase, KillLog }
    public struct ServerMessage : NetworkMessage
    {
        public MessageType messageType;
        public string message;
        public string subMessage;
    }

    public class OnGetServerMessage : UnityEvent<ServerMessage> { }

    public class ClientManager : NetworkManager
    {
        public static ClientManager i;
        public static OnGetServerMessage OnGetServerMessage = new OnGetServerMessage();
        public override List<GameObject> spawnPrefabs => this.serverData.spawnablePrefabs;
        [SerializeField]
        private ServerData serverData;

        private bool isRegistrationSceneRequired;
        private bool isRedirected;

        public override void Awake()
        {
            if (i != null && i != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                i = this;
            }
        }

        public override void Start()
        {
            base.Start();
            OnGetServerMessage.AddListener(ServerMessageNotificator.ShowMessage);
            OnGetServerMessage.AddListener(this.RedirectToAnotherServer);
            OnGetServerMessage.AddListener(this.RegistrationRequired);
            OnGetServerMessage.AddListener(this.DatabaseIntialize);
        }

        private void DatabaseIntialize(ServerMessage message)
        {
            if (message.messageType == MessageType.ItemDatabase)
            {
                ItemDatabase.DatabaseLoadJson(message.message);
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            NetworkClient.RegisterHandler<ServerMessage>(this.ServerMessageEvent, false);
        }

        private void ServerMessageEvent(ServerMessage msg)
        {
            OnGetServerMessage.Invoke(msg);
        }

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            print("Disconnect");
            if (!this.isRedirected)
            {
                LoginSceneUI.ShowPlayButton();
            }

            if (!this.isRegistrationSceneRequired)
            {
                SceneManager.LoadSceneAsync("LoginScene");
            }

            this.isRedirected = false;
            Mouse.EnableCursor();
        }

        private async void RedirectToAnotherServer(ServerMessage msg)
        {
            if (msg.messageType != MessageType.ServerRedirect)
            {
                return;
            }

            print("Redirect");
            this.isRedirected = true;
            LoginSceneUI.HidePlayButton();
            this.GetComponent<NetworkManager>().StopClient();
            await Task.Delay(1000);
            string port = msg.message.Split(':')[1];
            (this.transport as TelepathyTransport).port = ushort.Parse(port);
            this.networkAddress = msg.message.Split(':')[0];
#if UNITY_EDITOR
            UnityWebRequest www = UnityWebRequest.Get("ifconfig.me/all.json");
            await www.SendWebRequest();
            SimpleJSON.JSONNode response = SimpleJSON.JSON.Parse(www.downloadHandler.text);
            string externalIpAddres = response["ip_addr"].Value;
            if (externalIpAddres == this.networkAddress)
            {
                this.networkAddress = "localhost";
            }
#endif
            this.GetComponent<NetworkManager>().StartClient();
        }

        private void RegistrationRequired(ServerMessage msg)
        {
            if (msg.messageType == MessageType.RegistrationRequired)
            {
                this.isRegistrationSceneRequired = true;
                UILoadingOverlayManager.Instance.Create().LoadSceneAsync("FractionSelect");
            }
        }

        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
        {
            if (SceneManager.GetActiveScene().name == "LoginScene")
            {
                LoginSceneUI.HidePlayButton();
            }
        }

        public override void OnClientError(Exception exception)
        {
            print(exception.Message);
        }
    }

}