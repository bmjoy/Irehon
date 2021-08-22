using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

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
            NetworkClient.RegisterHandler<CharactersInfo>(SaveCharacter, true);
        }

        private void ServerMessageEvent(ServerMessage msg)
        {
            OnGetServerMessage.Invoke(msg);
        }

        private void SaveCharacter(CharactersInfo charactersInfo)
        {
            print(charactersInfo.characters.Length);
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

        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
        {
            base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
            NetworkClient.PrepareToSpawnSceneObjects();
        }
    }

}