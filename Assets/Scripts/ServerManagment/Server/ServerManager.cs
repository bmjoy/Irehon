using Mirror;
using Client;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Threading;
using UnityEngine.Networking;

namespace Server
{
    public class ServerManager : NetworkManager
    {
        private const int MAX_CHARACTERS_PER_ACCOUNT = 7;
        public static ServerManager i;

        [SerializeField]
        private Vector3 characterSpawnPoint;
        private NetworkManager manager;
        [SerializeField]
        private GameObject mob;
        public static List<int> ConnectedPlayers => i.connectedPlayersId;
        private List<int> connectedPlayersId = new List<int>();
        private Dictionary<int, Player> connectedCharacters = new Dictionary<int, Player>();

        public override void Awake()
        {
            if (i != null && i != this || ClientManager.i != null)
                Destroy(gameObject);
            else
                i = this;

            
            StartCoroutine(LoadDatabase());
        }

        IEnumerator LoadDatabase()
        {
            var www = Api.Connection.Request("https://irehon.com/items.php");
            yield return www.SendWebRequest();
            if (www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else
            {
                ItemDatabase.DatabaseLoadJson(www.downloadHandler.text);
            }
        }

        public override void Start()
        {
            manager = GetComponent<NetworkManager>();
            manager.clientLoadedScene = false;
            manager.StartServer();
            NetworkServer.RegisterHandler<CharacterOperationRequest>(CharacterOperationRequest, true);
            InvokeRepeating("UpdateAllDataCycle", 90, 90);
        }

        public void UpdateAllDataCycle()
        {
            UpdateDatabase();
        }

        public static Task UpdateDatabase()
        {
            return Api.ContainerData.UpdateDatabaseLoadedContainers();
        }

        public override void OnDestroy()
        {
        }

        public Player GetPlayer(int id)
        {
            return connectedCharacters[id];
        }

        public bool IsPlayerConnected(int p_id) => connectedPlayersId.Contains(p_id);

        public void CharacterOperationRequest(NetworkConnection con, CharacterOperationRequest request)
        {
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
                PlayerConnectionInfo data = (PlayerConnectionInfo)con.authenticationData;

                if (request.selectedSlot >= data.characters.Count)
                    return;
                Character selectedCharacter = data.characters[request.selectedSlot];

                int c_id = 0;

                c_id = Api.Requests.GetCharacterId(selectedCharacter.name);

                Api.Requests.DeleteCharacter(c_id);

                SendPlayerInfo(con);
            });
        }

        //Insert data in tables and send it to player
        private void PlayerCharacterCreateRequest(NetworkConnection con, CharacterOperationRequest character)
        {
            StartCoroutine(CreateRequest());
            IEnumerator CreateRequest()
            {
                PlayerConnectionInfo data = (PlayerConnectionInfo)con.authenticationData;
                
                if (data.characters.Length > MAX_CHARACTERS_PER_ACCOUNT)
                    yield break;

                if (!ServerAuth.IsLoginValid(character.nickname))
                {
                    SendMessage(con, "Invalid symbols in nickname", MessageType.Error);
                    yield break;
                }

                if (Api.Requests.GetCharacterId(character.nickname) != 0)
                {
                    SendMessage(con, "Nickname already in use", MessageType.Error);
                    return;
                }

                Character newCharacter = new Character
                {
                    name = character.nickname,
                    position = characterSpawnPoint,
                };
                Api.Requests.CreateNewCharacter(p_id, newCharacter);
                SendPlayerInfo(con);
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
                if (request.selectedSlot > MAX_CHARACTERS_PER_ACCOUNT)
                    yield break;

                ChangeScene(con, "PvpScene");

                PlayerConnectionInfo data = (PlayerConnectionInfo)con.authenticationData;

                if (request.selectedSlot >= data.characters.Length)
                    yield break;

                var www = Api.Request($"/characters/{data.characters[request.selectedSlot].id}");

                yield return www.SendWebRequest();

                CharacterInfo characterInfo = new CharacterInfo(Api.GetResult(www));

                data.selectedCharacter = characterInfo;

                SpawnPlayerOnMap(con, characterInfo);
                
                con.authenticationData = data;
            }
        }

        public GameObject SpawnPlayerOnMap(NetworkConnection con, CharacterInfo characterInfo)
        {
            GameObject playerObject = Instantiate(playerPrefab);

            playerObject.transform.position = characterInfo.position;

            Player playerComponent = playerObject.GetComponent<Player>();

            connectedCharacters.Add(characterInfo.id, playerComponent);

            playerComponent.SetName(characterInfo.name);

            NetworkServer.AddPlayerForConnection(con, playerObject);

            playerComponent.GetComponent<PlayerContainerController>().SendItemDatabase(ItemDatabase.jsonString);

            playerComponent.SendCharacterInfo(characterInfo);

            return playerObject;
        }

        //Send characters and make him to choose one of them
        public override void OnServerConnect(NetworkConnection con)
        {
            PlayerConnectionInfo data = (PlayerConnectionInfo)con.authenticationData;
            connectedPlayersId.Add(data.playerId);
            ChangeScene(con, "CharacterSelection");

            SendPlayerInfo(con);
        }

        //Character list to choose on of them
        private void SendPlayerInfo(NetworkConnection con) => con.Send((PlayerConnectionInfo)con.authenticationData);
        
        private IEnumerator UpdatePlayerInfo(NetworkConnection con)
        {
            PlayerConnectionInfo data = (PlayerConnectionInfo)con.authenticationData;
            var www = Api.Request($"/users/{data.playerId}");
            yield return www;
            con.authenticationData = new PlayerConnectionInfo(Api.GetResult(www));
        }

        public override void OnServerAddPlayer(NetworkConnection conn) { }

        public override void OnStopServer()
        {
            base.OnStopServer();
            UpdateDatabase().Wait();
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
            PlayerConnectionInfo data = (PlayerConnectionInfo)conn.authenticationData;
            var outer = Task.Factory.StartNew(() =>
            {
                if (data.characterId >= 0)
                {
                    int c_id = data.characterId;
                    Player player = data.playerPrefab.GetComponent<Player>();
                    Api.Requests.UpdatePositionData(c_id, player.transform.position);
                    Api.Requests.UpdateCharacterData(c_id, player.GetCharacterData());
                }
            });
            connectedCharacters.Remove(data.characterId);
            connectedPlayersId.Remove(data.playerId);
            base.OnServerDisconnect(conn);
        }

        private void ChangeScene(NetworkConnection con, string scene) => 
            con.Send(new SceneMessage{sceneName = "CharacterSelection",});
    }
}