using Mirror;
using Client;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Threading;
using UnityEngine.Networking;
using Steamworks;
using UnityEngine.SceneManagement;

namespace Server
{
    public class ServerManager : NetworkManager
    {
        public static ServerManager i;

        [SerializeField]
        private Vector3 characterSpawnPoint;
        [SerializeField]
        private GameObject mob;
        [SerializeField]
        private ServerData serverData;

        public override List<GameObject> spawnPrefabs => serverData.spawnablePrefabs;

        private NetworkManager manager;
        private Dictionary<ulong, NetworkConnection> connections;


        public override void Awake()
        {
            if (i != null && i != this || ClientManager.i != null)
                Destroy(gameObject);
            else
                i = this;
            
            LoadDatabase();
        }

        public override void Start()
        {
            manager = GetComponent<NetworkManager>();
            manager.clientLoadedScene = false;
            manager.StartServer();
            InvokeRepeating("UpdateAllDataCycle", 90, 90);
        }

        async void LoadDatabase()
        {
            var www = Api.Request("/items");
            await www.SendWebRequest();
            var json = Api.GetResult(www).ToString();

            Debug.Log("Loaded item database");

            ItemDatabase.DatabaseLoadJson(json);

            www = Api.Request("/recipes");
            await www.SendWebRequest();
            json = Api.GetResult(www).ToString();
            CraftDatabase.DatabaseLoadJson(json);
            Debug.Log("Loaded crafts database");

        }

        public void UpdateAllDataCycle()
        {
            UpdateDatabase();
        }

        public static void UpdateDatabase()
        {
            ContainerData.UpdateDatabaseLoadedContainers();
        }
        //Insert data in tables and send it to player
        private async Task PlayerCharacterCreateRequest(NetworkConnection con)
        {
            PlayerConnectionInfo data = (PlayerConnectionInfo)con.authenticationData;

            var www = Api.Request($"/characters/?id={data.steamId}", ApiMethod.POST);

            await www.SendWebRequest();
        }

        //Spawn character on player selected slot 
        private async void PlayerPlayRequest(NetworkConnection con, CharacterInfo characterInfo)
        {
            PlayerConnectionInfo data = (PlayerConnectionInfo)con.authenticationData;

            Friend friend = new Friend(data.steamId);
            await friend.RequestInfoAsync();

            characterInfo.name = friend.Name;

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

            playerComponent.SetName(characterInfo.name);

            NetworkServer.AddPlayerForConnection(con, playerObject);

            playerComponent.GetComponent<PlayerContainerController>().SendItemDatabase(ItemDatabase.jsonString);

            playerComponent.SendCharacterInfo(characterInfo);

            return playerObject;
        }

        //Send characters and make him to choose one of them
        public override async void OnServerConnect(NetworkConnection con)
        {
            ChangeScene(con, SceneManager.GetActiveScene().name);
            PlayerConnectionInfo data = (PlayerConnectionInfo)con.authenticationData;

            var www = Api.Request($"/characters/{data.steamId}");

            await www.SendWebRequest();

            if (Api.GetResult(www) == null)
                await PlayerCharacterCreateRequest(con);

            www = Api.Request($"/characters/{data.steamId}");

            await www.SendWebRequest();

            PlayerPlayRequest(con, new CharacterInfo(Api.GetResult(www)));
        }

        public override void OnServerAddPlayer(NetworkConnection conn) { }

        public override void OnStopServer()
        {
            base.OnStopServer();
            StartCoroutine(UpdateDatabase());
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            GameObject mob = Instantiate(this.mob);
            NetworkServer.Spawn(mob);
        }

        private async void UpdateCharacterData(ulong id)
        {
            Player player = GetPlayer(id);
            Vector3 pos = player.transform.position;

            var www = Api.Request($"/characters/{id}?p_x={pos.x}&p_y={pos.y}&p_z={pos.z}", ApiMethod.PUT);
            await www.SendWebRequest();
        }

        private IEnumerator CharacterLeaveFromWorld(ulong id)
        {
            UpdateCharacterData(id);
            print($"Unspawnd character id{id}");
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

            if (data.selectedCharacter.id != 0)
                StartCoroutine(CharacterLeaveFromWorld(data.selectedCharacter.id));
                
            print($"Disconnect player id{data.steamId}");
            connections.Remove(data.steamId);
            base.OnServerDisconnect(conn);
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

        public void RemoveUserFromConnections(ulong steamId) => connections.Remove(steamId);

        public void AddConection(ulong steamId, NetworkConnection con) => connections[steamId] = con;

        public NetworkConnection GetConnection(ulong steamId) => connections.ContainsKey(steamId) ? connections[steamId] : null;
    }
}