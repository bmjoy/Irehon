using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System;
using System.Net.Sockets;
using DuloGames.UI;
using kcp2k;
using System.Threading.Tasks;

namespace Client
{
    public enum MessageType { AuthAccept, AuthReject, Error, Notification, RegistrationRequired, ServerRedirect, ItemDatabase }
    public struct ServerMessage : NetworkMessage
    {
        public MessageType messageType;
        public string message;
    }

    //public class OnUpdateCharacterList : UnityEvent<List<Character>> { }

    public class OnGetServerMessage : UnityEvent<ServerMessage> { }

    public class ClientManager : NetworkManager
    {
        public static ClientManager i;
        //public OnUpdateCharacterList OnUpdateCharacterList;
        public static OnGetServerMessage OnGetServerMessage = new OnGetServerMessage();
        public override List<GameObject> spawnPrefabs => serverData.spawnablePrefabs;
        [SerializeField]
        private ServerData serverData;

        private bool isRegistrationSceneRequired;
        private bool isRedirected;

        public override void Awake()
        {
            if (i != null && i != this)
                Destroy(gameObject);
            else
                i = this;
        }

        override public void OnDestroy()
        {
            //Shutdown();

        }

        public override void Start()
        {
            base.Start();
            OnGetServerMessage.AddListener(ServerMessageNotificator.ShowMessage);
            OnGetServerMessage.AddListener(RedirectToAnotherServer);
            OnGetServerMessage.AddListener(RegistrationRequired);
            OnGetServerMessage.AddListener(DatabaseIntialize);
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
            NetworkClient.RegisterHandler<ServerMessage>(ServerMessageEvent, false);
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
            CameraController.EnableCursor();
            print("Disconnect, isRedirected = " + isRedirected);
            if (!isRedirected)
                LoginSceneUI.ShowPlayButton();
            if (!isRegistrationSceneRequired)
                SceneManager.LoadScene("LoginScene");
            isRedirected = false;
        }

        private async void RedirectToAnotherServer(ServerMessage msg)
        {
            if (msg.messageType != MessageType.ServerRedirect)
                return;
            print("Redirect");
            isRedirected = true;
            LoginSceneUI.HidePlayButton();
            GetComponent<NetworkManager>().StopClient();
            await Task.Delay(1000);
            string port = msg.message.Split(':')[1];
            (transport as KcpTransport).Port = ushort.Parse(port);
            networkAddress = msg.message.Split(':')[0];
            GetComponent<NetworkManager>().StartClient();
        }

        private void RegistrationRequired(ServerMessage msg)
        {
            if (msg.messageType == MessageType.RegistrationRequired)
            {
                isRegistrationSceneRequired = true;
                UILoadingOverlayManager.Instance.Create().LoadSceneAsync("FractionSelect");
            }
        }

        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
        {
            if (SceneManager.GetActiveScene().name == "LoginScene")
                LoginSceneUI.HidePlayButton();
            NetworkClient.PrepareToSpawnSceneObjects();
        }

        public override void OnClientError(Exception exception)
        {
        }
    }

}