using Mirror;
using Client;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Server
{
    public class ServerManager : NetworkManager
    {
        public static ServerManager i;

        [SerializeField]
        private Vector3 characterSpawnPoint;
        private NetworkManager manager;
        [SerializeField]
        private GameObject mob;

        private List<int> connectedPlayersId = new List<int>();
        private Dictionary<int, Player> connectedCharacters = new Dictionary<int, Player>();

        public override void Awake()
        {
            if (i != null && i != this || ClientManager.i != null)
                Destroy(gameObject);
            else
                i = this;
        }

        public override void Start()
        {
            manager = GetComponent<NetworkManager>();
            manager.clientLoadedScene = false;
            manager.StartServer();
            ItemDatabase.DatabaseLoad();
            NetworkServer.RegisterHandler<CharacterOperationRequest>(CharacterOperationRequest, true);
        }

        public override void OnDestroy()
        {
            //MySql.Connection.Shutdown();
            //Shutdown();
        }

        public Player GetPlayer(int id)
        {
            return connectedCharacters[id];
        }

        public bool IsPlayerConnected(int p_id) => connectedPlayersId.Contains(p_id);

        public void CharacterOperationRequest(NetworkConnection con, CharacterOperationRequest request)
        {
            print(request.opeartion);
            switch (request.opeartion)
            {
                case CharacterOperations.Create:
                    PlayerCharacterCreateRequest(con, request);
                    return;
                case CharacterOperations.Play:
                    PlayerPlayRequest(con, request);
                    return;
                case CharacterOperations.Delete:
                    PlayerDeleteRequest(con, request);
                    return;
            }
        }

        private void PlayerDeleteRequest(NetworkConnection con, CharacterOperationRequest request)
        {
            Task.Factory.StartNew(() =>
            {
                PlayerConnection data = (PlayerConnection)con.authenticationData;

                if (request.selectedSlot > data.characters.Count)
                    return;
                Character selectedCharacter = data.characters[request.selectedSlot];

                int c_id = 0;

                c_id = MySql.Database.GetCharacterId(selectedCharacter.name);

                MySql.Database.DeleteCharacter(c_id);

                SendCharacterListToPlayer(con);
            });
        }

        //Insert data in tables and send it to player
        private void PlayerCharacterCreateRequest(NetworkConnection con, CharacterOperationRequest character)
        {
            var outer = Task.Factory.StartNew(() =>
            {
                PlayerConnection data = (PlayerConnection)con.authenticationData;
                
                int p_id = data.playerId;

                if (data.characters.Count > MySql.Database.MAX_CHARACTERS_PER_ACCOUNT)
                    return;

                if (!ServerAuth.IsLoginValid(character.nickname))
                {
                    SendMessage(con, "Invalid symbols in nickname", MessageType.Error);
                    return;
                }

                if (MySql.Database.GetCharacterId(character.nickname) != 0)
                {
                    SendMessage(con, "Nickname already in use", MessageType.Error);
                    return;
                }

                Character newCharacter = new Character
                {
                    name = character.nickname,
                    position = characterSpawnPoint,
                };
                MySql.Database.CreateNewCharacter(p_id, newCharacter);
                SendCharacterListToPlayer(con);
            });
        }

        public static void SendMessage(NetworkConnection con, string msg, MessageType type)
        {
            ServerMessage serverMessage = new ServerMessage
            {
                message = msg,
                messageType = type
            };
            con.Send(serverMessage);
        }

        //Spawn character on player selected slot 
        private void PlayerPlayRequest(NetworkConnection con, CharacterOperationRequest request)
        {
            StartCoroutine(SpawnPlayer());
            IEnumerator SpawnPlayer()
            {
                if (request.selectedSlot > MySql.Database.MAX_CHARACTERS_PER_ACCOUNT)
                    yield break;
                SceneMessage message = new SceneMessage
                {
                    sceneName = "PvpScene",
                };
                con.Send(message);

                PlayerConnection data = (PlayerConnection)con.authenticationData;

                if (request.selectedSlot > data.characters.Count)
                    yield break;

                Character selectedCharacter = data.characters[request.selectedSlot];

                int c_id = 0;

                var outer = Task.Factory.StartNew(() => c_id = MySql.Database.GetCharacterId(selectedCharacter.name));
                while (!outer.IsCompleted)
                    yield return null;

                if (c_id == 0)
                    yield break;

                data.characterId = c_id;

                GameObject playerObject = Instantiate(playerPrefab);
                playerObject.transform.position = selectedCharacter.position;

                Player playerComponent = playerObject.GetComponent<Player>();

                connectedCharacters.Add(c_id, playerComponent);

                playerComponent.SetName(selectedCharacter.name);

                data.playerPrefab = playerObject.transform;

                NetworkServer.AddPlayerForConnection(con, playerObject);

                playerComponent.GetComponent<PlayerContainerController>().SendItemDatabase(ItemDatabase.jsonString);

                CharacterData characterData = new CharacterData();

                var charDataTask = Task.Factory.StartNew(() => characterData = MySql.Database.GetCharacterData(c_id));

                while (!charDataTask.IsCompleted)
                    yield return null;
                playerComponent.SetCharacterData(characterData);

                con.authenticationData = data;
            }
        }

        //Send characters and make him to choose one of them
        public override void OnServerConnect(NetworkConnection conn)
        {
            var outer = Task.Factory.StartNew(() =>
            {
                PlayerConnection data = (PlayerConnection)conn.authenticationData;

                if (connectedPlayersId.Contains(data.playerId))
                {
                    SendMessage(conn, "Already connected", MessageType.Error);
                    StartCoroutine(WaitBeforeDisconnect());
                    IEnumerator WaitBeforeDisconnect()
                    {
                        yield return new WaitForSeconds(0.05f);
                        conn.Disconnect();
                    }
                    return;
                }
                else
                    connectedPlayersId.Add(data.playerId);
                SceneMessage message = new SceneMessage
                {
                    sceneName = "CharacterSelection",
                };
                conn.Send(message);

                SendCharacterListToPlayer(conn);
            });
        }

        //Character list to choose on of them
        private void SendCharacterListToPlayer(NetworkConnection con)
        {
            PlayerConnection data = (PlayerConnection)con.authenticationData;
            data.characters = MySql.Database.GetCharacters(data.playerId);
            foreach (Character character in data.characters)
            {
                con.Send(character);

            }
            con.authenticationData = data;
        }

        public override void OnServerAddPlayer(NetworkConnection conn) { }

        public override void OnStopServer()
        {
            base.OnStopServer();

        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            GameObject mob = Instantiate(this.mob);
            NetworkServer.Spawn(mob);
        }

        //Update character data on DB on disconneect
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            if (conn.authenticationData == null)
            {
                base.OnServerDisconnect(conn);
                return;
            }
            PlayerConnection data = (PlayerConnection)conn.authenticationData;
            var outer = Task.Factory.StartNew(() =>
            {
                if (data.characterId >= 0)
                {
                    int c_id = data.characterId;
                    Player player = data.playerPrefab.GetComponent<Player>();
                    MySql.Database.UpdatePositionData(c_id, player.transform.position);
                    MySql.Database.UpdateCharacterData(c_id, player.GetCharacterData());
                }
            });
            connectedCharacters.Remove(data.characterId);
            connectedPlayersId.Remove(data.playerId);
            base.OnServerDisconnect(conn);
        }
    }
}