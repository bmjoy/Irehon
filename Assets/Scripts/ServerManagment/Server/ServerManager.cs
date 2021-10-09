using Mirror;
using Client;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Threading;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Server
{
    public class ServerManager : NetworkManager
    {
        private const int MAX_CHARACTERS_PER_ACCOUNT = 7;

        public static ServerManager i;

        [SerializeField]
        private Vector3 characterSpawnPoint;
        [SerializeField]
        private GameObject mob;
        [SerializeField]
        private ServerData serverData;

        public override List<GameObject> spawnPrefabs => serverData.spawnablePrefabs;

        private NetworkManager manager;
        private Dictionary<ulong, Player> connectedCharacters;

        public override void Awake()
        {
            if (i != null && i != this || ClientManager.i != null)
                Destroy(gameObject);
            else
                i = this;

            connectedCharacters = new Dictionary<ulong, Player>();
            
            StartCoroutine(LoadDatabase());
        }

        public override void Start()
        {
            manager = GetComponent<NetworkManager>();
            manager.clientLoadedScene = false;
            manager.StartServer();
            InvokeRepeating("UpdateAllDataCycle", 90, 90);
        }

        IEnumerator LoadDatabase()
        {
            var www = Api.Request("/items");
            yield return www.SendWebRequest();
            var json = Api.GetResult(www).ToString();

            Debug.Log("Loaded item database");

            ItemDatabase.DatabaseLoadJson(json);

            www = Api.Request("/recipes");
            yield return www.SendWebRequest();
            json = Api.GetResult(www).ToString();
            CraftDatabase.DatabaseLoadJson(json);
            Debug.Log("Loaded crafts database");

        }

        public void UpdateAllDataCycle()
        {
            StartCoroutine(UpdateDatabase());
        }

        public static IEnumerator UpdateDatabase()
        {
            yield return ContainerData.UpdateDatabaseLoadedContainers();
        }
        //Insert data in tables and send it to player
        private IEnumerator PlayerCharacterCreateRequest(NetworkConnection con)
        {
            PlayerConnectionInfo data = (PlayerConnectionInfo)con.authenticationData;

            var www = Api.Request($"/characters/?id={data.steamId}", ApiMethod.POST);

            yield return www.SendWebRequest();
        }

        //Spawn character on player selected slot 
        private IEnumerator PlayerPlayRequest(NetworkConnection con, ulong id)
        {
            ChangeScene(con, "DevTestScene");

            PlayerConnectionInfo data = (PlayerConnectionInfo)con.authenticationData;

            var www = Api.Request($"/characters/{id}");

            yield return www.SendWebRequest();

            CharacterInfo characterInfo = new CharacterInfo(Api.GetResult(www));

            data.selectedCharacter = characterInfo;

            SpawnPlayerOnMap(con, characterInfo);
                
            con.authenticationData = data;
        }

        public GameObject SpawnPlayerOnMap(NetworkConnection con, CharacterInfo characterInfo)
        {
            GameObject playerObject = Instantiate(playerPrefab);

            playerObject.transform.position = characterInfo.position;

            Player playerComponent = playerObject.GetComponent<Player>();

            print($"Spawning character id{characterInfo.id}");
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
            ChangeScene(con, SceneManager.GetActiveScene().name);
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
            yield return www.SendWebRequest();
        }

        private IEnumerator CharacterLeaveFromWorld(int id)
        {
            yield return UpdateCharacterData(id);
            print($"Unspawnd character id{id}");
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
                
                print($"Disconnect player id{data.steamId}");
                connectedPlayersId.Remove(data.steamId);
                base.OnServerDisconnect(conn);
            }
        }

        private void ChangeScene(NetworkConnection con, string scene)
        {
            con.Send(new SceneMessage { sceneName = scene, });
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

        public Player GetPlayer(ulong steamId) => connectedCharacters.ContainsKey(steamId) ? connectedCharacters[steamId] : null;

        public bool IsPlayerConnected(ulong p_id) => connectedPlayersId.Contains(p_id);
    }
}