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

        public static List<int> ConnectedPlayers => i.connectedPlayersId;
        public static ServerManager i;

        [SerializeField]
        private Vector3 characterSpawnPoint;
        [SerializeField]
        private GameObject mob;

        private NetworkManager manager;
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

        public override void Start()
        {
            manager = GetComponent<NetworkManager>();
            manager.clientLoadedScene = false;
            manager.StartServer();
            NetworkServer.RegisterHandler<CharacterOperationRequest>(CharacterOperationRequest, true);
            InvokeRepeating("UpdateAllDataCycle", 90, 90);
        }

        IEnumerator LoadDatabase()
        {
            var www = Api.Request("/items");
            yield return www.SendWebRequest();
            var json = Api.GetResult(www).ToString();

            ItemDatabase.DatabaseLoadJson(json);

            www = Api.Request("/recipes");
            yield return www.SendWebRequest();
            json = Api.GetResult(www).ToString();
            CraftDatabase.DatabaseLoadJson(json);
        }

        public void UpdateAllDataCycle()
        {
            StartCoroutine(UpdateDatabase());
        }

        public static IEnumerator UpdateDatabase()
        {
            yield return ContainerData.UpdateDatabaseLoadedContainers();
        }

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
            StartCoroutine(Delete());
            IEnumerator Delete()
            {
                PlayerConnectionInfo data = (PlayerConnectionInfo)con.authenticationData;

                if (request.selectedSlot >= data.characters.Length)
                    yield break;
                Character selectedCharacter = data.characters[request.selectedSlot];

                int c_id = selectedCharacter.id;

                var www = Api.Request($"/characters/{c_id}", ApiMethod.DELETE);
                yield return www.SendWebRequest();

                yield return SendPlayerInfo(con);
            }
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

                var www = Api.Request($"/characters/?name={character.nickname}&player_id={data.playerId}", ApiMethod.POST);

                yield return www.SendWebRequest();

                var res = Api.GetResult(www);

                if (res == null)
                {
                    SendMessage(con, "Nickname already in use", MessageType.Error);
                    yield break;
                }

                yield return SendPlayerInfo(con);
            }
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

            StartCoroutine(SendPlayerInfo(con));
        }

        //Character list to choose on of them
        private IEnumerator SendPlayerInfo(NetworkConnection con) 
        {
            yield return UpdatePlayerInfo(con);
            con.Send((PlayerConnectionInfo)con.authenticationData);
        }
        private IEnumerator UpdatePlayerInfo(NetworkConnection con)
        {
            PlayerConnectionInfo data = (PlayerConnectionInfo)con.authenticationData;
            var www = Api.Request($"/users/{data.playerId}");
            yield return www.SendWebRequest();
            con.authenticationData = new PlayerConnectionInfo(Api.GetResult(www));
        }

        public override void OnServerAddPlayer(NetworkConnection conn) { }

        public override void OnStopServer()
        {
            base.OnStopServer();
            StartCoroutine(OnStopServerCoroutine());

            IEnumerator OnStopServerCoroutine()
            {
                yield return UpdateDatabase();
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            GameObject mob = Instantiate(this.mob);
            NetworkServer.Spawn(mob);
        }

        private IEnumerator UpdateCharacterData(int id)
        {
            Player player = GetPlayer(id);
            Vector3 pos = player.transform.position;
            var www = Api.Request($"/characters/{id}?p_x={pos.x}&p_y={pos.y}&p_z={pos.z}", ApiMethod.PUT);
            print(www.uri);
            yield return www.SendWebRequest();
            print(www.responseCode);
        }

        private IEnumerator CharacterLeaveFromWorld(int id)
        {
            yield return UpdateCharacterData(id);

            connectedCharacters.Remove(id);
        }

        //Update character data on DB on disconneect
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            StartCoroutine(ServerDisconnect());
            IEnumerator ServerDisconnect()
            {
                if (conn.authenticationData == null)
                {
                    base.OnServerDisconnect(conn);
                    yield break;
                }

                PlayerConnectionInfo data = (PlayerConnectionInfo)conn.authenticationData;

                if (data.selectedCharacter.id != 0)
                    yield return CharacterLeaveFromWorld(data.selectedCharacter.id);

                connectedPlayersId.Remove(data.playerId);
                base.OnServerDisconnect(conn);
            }
        }

        private void ChangeScene(NetworkConnection con, string scene) => 
            con.Send(new SceneMessage{sceneName = scene,});

        public static void SendMessage(NetworkConnection con, string msg, MessageType type)
        {
            ServerMessage serverMessage = new ServerMessage
            {
                message = msg,
                messageType = type
            };
            con.Send(serverMessage);
        }

        public Player GetPlayer(int id) => connectedCharacters.ContainsKey(id) ? connectedCharacters[id] : null;

        public bool IsPlayerConnected(int p_id) => connectedPlayersId.Contains(p_id);
    }
}