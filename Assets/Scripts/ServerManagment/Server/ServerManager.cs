using Client;
using kcp2k;
using Mirror;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Server
{
    public class ServerManager : NetworkManager
    {
        public static ServerManager i;

        [SerializeField]
        private ServerData serverData;

        public override List<GameObject> spawnPrefabs => serverData.spawnablePrefabs;

        private Dictionary<ulong, NetworkConnection> connections;

        private int serverId;
        public override void Awake()
        {
            if (i != null && i != this || ClientManager.i != null)
                Destroy(gameObject);
            else
                i = this;
            connections = new Dictionary<ulong, NetworkConnection>();
            LoadDatabase();
        }

        public static void WaitBeforeDisconnect(NetworkConnection con)
        {
            IEnumerator WaitBeforeDisconnect()
            {
                yield return new WaitForSeconds(0.2f);
                con.Disconnect();
            }
            i.StartCoroutine(WaitBeforeDisconnect());
        }

        private async void CreateServerInDB()
        {
            var www = UnityWebRequest.Get("ifconfig.me/all.json");
            await www.SendWebRequest();
            SimpleJSON.JSONNode response = SimpleJSON.JSON.Parse(www.downloadHandler.text);
            string externalIpAddres = response["ip_addr"].Value;

            www = Api.Request($"/servers/?ip={externalIpAddres}" +
                $"&port={(transport as KcpTransport).Port}" +
                $"&location={SceneManager.GetActiveScene().name}", ApiMethod.POST);
            await www.SendWebRequest();
            serverId = Api.GetResult(www)["id"].AsInt;
        }

        public override void Start()
        {
            clientLoadedScene = false;
            (transport as KcpTransport).Port = ushort.Parse(Environment.GetEnvironmentVariable("PORT"));
            StartServer();
            InvokeRepeating("UpdatePlayerCount", 5, 5);
            InvokeRepeating("UpdateAllDataCycle", 90, 90);
        }

        private async void UpdatePlayerCount()
        {
            if (serverId == 0)
                return;
            var www = Api.Request($"/servers/?id={serverId}&online={connections.Count}", ApiMethod.PUT);
            await www.SendWebRequest();
        }

        private async void LoadDatabase()
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
        private async Task<bool> PlayerCharacterCreateRequest(NetworkConnection con)
        {
            PlayerConnectionInfo data = (PlayerConnectionInfo)con.authenticationData;

            if (data.authInfo.registerInfo.fraction != Fraction.None)
            {
                var www = Api.Request($"/characters/?steam_id={data.steamId}&fraction={data.authInfo.registerInfo.fraction}", ApiMethod.POST);

                await www.SendWebRequest();

                print("Character created");

                return true;
            }
            else
            {
                SendMessage(con, "Create character", MessageType.RegistrationRequired);
                print("Sended character create request");
            }
            return false;
        }

        //Spawn character on player selected slot 
        private async void PlayerPlayRequest(NetworkConnection con, CharacterInfo characterInfo)
        {
            PlayerConnectionInfo data = (PlayerConnectionInfo)con.authenticationData;

            if (characterInfo.serverId != serverId)
            {
                if (characterInfo.serverId == 0)
                {
                    SendMessage(con, "Server unavalible", MessageType.Notification);
                    WaitBeforeDisconnect(con);
                    return;
                }

                var www = Api.Request($"/servers/{characterInfo.serverId}");

                await www.SendWebRequest();

                var result = Api.GetResult(www);

                print("Redirected");

                SendMessage(con, $"{result["ip"]}:{result["port"]}", MessageType.ServerRedirect);
                WaitBeforeDisconnect(con);
                return;
            }

            ChangeScene(con, SceneManager.GetActiveScene().name);

            data.character = characterInfo;

            GameObject player = SpawnPlayerOnMap(con, characterInfo);

            data.playerPrefab = player.transform;

            con.authenticationData = data;
        }

        public GameObject SpawnPlayerOnMap(NetworkConnection con, CharacterInfo characterInfo)
        {
            GameObject playerObject = Instantiate(playerPrefab);

            playerObject.transform.position = characterInfo.position;

            Player playerComponent = playerObject.GetComponent<Player>();

            print($"Spawning character id{characterInfo.id}");

            playerComponent.SetName(characterInfo.name);

            playerComponent.Id = characterInfo.id;

            NetworkServer.AddPlayerForConnection(con, playerObject);

            playerComponent.GetComponent<PlayerContainerController>().SendItemDatabase(ItemDatabase.jsonString);

            playerComponent.SendCharacterInfo(characterInfo);

            return playerObject;
        }

        //Send characters and make him to choose one of them
        public override async void OnServerConnect(NetworkConnection con)
        {
            PlayerConnectionInfo data = (PlayerConnectionInfo)con.authenticationData;

            var www = Api.Request($"/characters/{data.steamId}");

            await www.SendWebRequest();

            if (Api.GetResult(www) == null)
            {
                print("Player not found");
                bool isCreated = await PlayerCharacterCreateRequest(con);
                if (!isCreated)
                {
                    WaitBeforeDisconnect(con);
                    return;
                }

                www = Api.Request($"/characters/{data.steamId}");

                await www.SendWebRequest();
            }

            print("Player play request start");

            PlayerPlayRequest(con, new CharacterInfo(Api.GetResult(www)));
        }

        public override void OnServerAddPlayer(NetworkConnection conn) { }

        public override async void OnStopServer()
        {
            base.OnStopServer();
            await DeleteServerInDB();
            UpdateDatabase();
        }

        private async Task DeleteServerInDB()
        {
            var www = Api.Request($"/servers/{serverId}", ApiMethod.DELETE);
            await www.SendWebRequest();
            serverId = 0;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            SteamManager.StartServer();
            CreateServerInDB();
        }

        private async Task UpdateCharacterData(CharacterInfo info, Transform player)
        {
            if (!info.isOnlineOnAnotherServer)
                return;

            Vector3 pos;
            if (info.sceneChangeInfo != null)
            {
                pos = info.sceneChangeInfo.spawnPosition;
                info.location = info.sceneChangeInfo.sceneName;
            }
            else
                pos = player.transform.position;
            
            var www = Api.Request($"/characters/{info.id}?p_x={pos.x}&p_y={pos.y}&p_z={pos.z}&location={info.location}", ApiMethod.PUT);
            await www.SendWebRequest();

            if (info.isSpawnPointChanged)
            {
                www = Api.Request($"/characters/{info.id}?sp_x={info.spawnPoint.x}&sp_y={info.spawnPoint.y}&p_z={info.spawnPoint.z}&sp_location={info.spawnSceneName}", ApiMethod.PUT);
                await www.SendWebRequest();
            }
        }

        private async Task CharacterLeaveFromWorld(PlayerConnectionInfo info)
        {
            await UpdateCharacterData(info.character, info.playerPrefab);
            print($"Unspawned character id{info.character.id}");
        }

        //Update character data on DB on disconneect
        public override async void OnServerDisconnect(NetworkConnection conn)
        {
            if (conn.authenticationData == null)
            {
                base.OnServerDisconnect(conn);
                return;
            }

            PlayerConnectionInfo data = (PlayerConnectionInfo)conn.authenticationData;

            if (data.character.id != 0)
                await CharacterLeaveFromWorld(data);
            SteamServer.EndSession(data.steamId);
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