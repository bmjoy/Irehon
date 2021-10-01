using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System;
using System.Net.Sockets;
using DuloGames.UI;

namespace Client
{
    public enum MessageType { AuthAccept, AuthReject, Error, Notification }
    public struct ServerMessage : NetworkMessage
    {
        public MessageType messageType;
        public string message;
    }

    public struct CharactersInfo : NetworkMessage
    {
        public Character[] characters;
    }

    public class OnUpdateCharacterList : UnityEvent<List<Character>> { }

    public class OnGetServerMessage : UnityEvent<ServerMessage> { }

    public class ClientManager : NetworkManager
    {
        public static ClientManager i;
        public OnUpdateCharacterList OnUpdateCharacterList;
        public static OnGetServerMessage OnGetServerMessage = new OnGetServerMessage();
        private List<Character> charactersList = new List<Character>();
        public override List<GameObject> spawnPrefabs => serverData.spawnablePrefabs;
        [SerializeField]
        private ServerData serverData;

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
            if (OnUpdateCharacterList == null)
                OnUpdateCharacterList = new OnUpdateCharacterList();
            OnGetServerMessage.AddListener(ServerMessageNotificator.ShowMessage);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            NetworkClient.RegisterHandler<ServerMessage>(ServerMessageEvent, false);
            NetworkClient.RegisterHandler<PlayerConnectionInfo>(SaveCharacter, true);
        }

        private void ServerMessageEvent(ServerMessage msg)
        {
            OnGetServerMessage.Invoke(msg);
        }

        private void SaveCharacter(PlayerConnectionInfo charactersInfo)
        {
            charactersList = new List<Character>(charactersInfo.characters);
            OnUpdateCharacterList.Invoke(charactersList);
        }

        public List<Character> GetCharacters() => charactersList;

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
        }

        protected override void ChangeScene(string scene, AsyncOperation ao)
        {
            UILoadingOverlayManager.Instance.Create().LoadSceneAsync(scene);
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            CameraController.EnableCursor();
            SceneManager.LoadScene("LoginScene");
        }

        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
        {
            base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
            NetworkClient.PrepareToSpawnSceneObjects();
        }

        public override void OnClientError(Exception exception)
        {
        }
    }

}