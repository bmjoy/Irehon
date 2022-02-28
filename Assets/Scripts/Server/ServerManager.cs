using Irehon.CloudAPI;
using Irehon.Client;
using Irehon.Utils;
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
using Irehon.Entitys;
using Irehon;
using Irehon.Steam;

namespace Irehon
{
    public class ServerManager : NetworkManager
    {
        public static ServerManager Instance;

        [SerializeField]
        private ServerData serverData;

        public override List<GameObject> spawnPrefabs => serverData.spawnablePrefabs;

        public List<NetworkConnection> connections;

        public bool isWhiteListEnabled = false;

        private int serverId;
        private ushort port;

        public override void Awake()
        {
            if (Instance != null && Instance != this || ClientManager.i != null)
                Destroy(gameObject);
            else
            {
                Instance = this;
                connections = new List<NetworkConnection>();
                LoadDatabase();
            }
        }

        public static void Log(SteamId id, string message) => Debug.Log($"{DateTime.Now} [{id}] {message}");
        public static void Log(string message) => Debug.Log($"{DateTime.Now} [server] {message}");


        public static void WaitBeforeDisconnect(NetworkConnection con)
        {
            Debug.Log("Disconnecting");
            IEnumerator WaitBeforeDisconnect()
            {
                yield return new WaitForSeconds(0.2f);
                con.Disconnect();
            }
            Instance.StartCoroutine(WaitBeforeDisconnect());
        }

        private async void CreateServerInDB()
        {
            var www = UnityWebRequest.Get("ifconfig.me/all.json");
            await www.SendWebRequest();
            SimpleJSON.JSONNode response = SimpleJSON.JSON.Parse(www.downloadHandler.text);
            string externalIpAddres = response["ip_addr"].Value;
            networkAddress = externalIpAddres;
            port = (transport as TelepathyTransport).port;

            www = Api.Request($"/servers?ip={externalIpAddres}" +
                $"&port={(transport as TelepathyTransport).port}" +
                $"&location={SceneManager.GetActiveScene().name}", ApiMethod.POST);
            await www.SendWebRequest();
            serverId = Api.GetResult(www)["id"].AsInt;
        }

        public override void Start()
        {
            clientLoadedScene = false;
            (transport as TelepathyTransport).port = ushort.Parse(Environment.GetEnvironmentVariable("SERVERPORT"));
            StartServer();
            InvokeRepeating(nameof(UpdatePlayerCount), 2, 2);
        }

        private async void UpdatePlayerCount()
        {
            if (serverId == 0)
                return;
            var www = Api.Request($"/servers/{serverId}?quantity={connections.Count}", ApiMethod.PUT);
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

        //Insert data in tables and send it to player
        private async Task<bool> PlayerCharacterCreateRequest(NetworkConnection con)
        {
            Irehon.PlayerSession data = (Irehon.PlayerSession)con.authenticationData;

            if (data.authInfo.registerInfo.fraction != Fraction.None)
            {
                var www = Api.Request($"/users/{data.steamId}?fraction={data.authInfo.registerInfo.fraction}", ApiMethod.POST);

                await www.SendWebRequest();

                Log(data.steamId, $"Character created {data.authInfo.registerInfo.fraction}");

                return true;
            }
            else
            {
                SendMessage(con, "Create character", MessageType.RegistrationRequired);
                Log(data.steamId, "Sended character create request");
            }
            return false;
        }

        //Spawn character on player selected slot 
        private async void PlayerPlayRequest(NetworkConnection con, CharacterInfo characterInfo)
        {
            Irehon.PlayerSession data = (Irehon.PlayerSession)con.authenticationData;

            if (characterInfo.isOnlineOnAnotherServer)
            {
                Log(data.steamId, $"Disconnect error: already connected");
                SendMessage(con, "Already connected to another server", MessageType.Notification);
                WaitBeforeDisconnect(con);
                return;
            }

            var www = Api.Request($"/balancer/{characterInfo.steamId}");

            await www.SendWebRequest();

            var result = Api.GetResult(www);

            int avaliableServerId = result == null ? 0 : result["id"].AsInt;

#if !UNITY_EDITOR
            if (avaliableServerId != serverId)
#else
            if (characterInfo.location != SceneManager.GetActiveScene().name)
#endif
            {
                if (avaliableServerId == 0)
                {
                    Log(data.steamId, $"Not founded server for {characterInfo.location} location");
                    SendMessage(con, "Server unavalible", MessageType.Notification);
                    WaitBeforeDisconnect(con);
                    return;
                }

                www = Api.Request($"/servers/{avaliableServerId}");

                await www.SendWebRequest();

                result = Api.GetResult(www);

                Log(data.steamId, $"Redirected to {result["ip"].Value}:{result["port"].Value}");

                SendMessage(con, $"{result["ip"].Value}:{result["port"].Value}", MessageType.ServerRedirect);
                WaitBeforeDisconnect(con);
                return;
            }

            ChangeScene(con, SceneManager.GetActiveScene().name); 

            data.character = characterInfo;

            await LoadPlayerContainers(data);

            GameObject player = SpawnPlayerOnMap(con, characterInfo);

            data.playerPrefab = player.transform;

            con.authenticationData = data;
        }

        public GameObject SpawnPlayerOnMap(NetworkConnection con, CharacterInfo characterInfo)
        {
            SendMessage(con, ItemDatabase.jsonString, MessageType.ItemDatabase);

            GameObject playerObject = Instantiate(playerPrefab);

            Player playerComponent = playerObject.GetComponent<Player>();

            Log(characterInfo.steamId, $"Spawning on map {characterInfo.position}");

            playerComponent.SetName(characterInfo.name);

            playerComponent.Id = characterInfo.steamId;

            NetworkServer.AddPlayerForConnection(con, playerObject);

            playerComponent.GetComponent<CharacterController>().SetPosition(characterInfo.position);
            playerComponent.SetPositionRpc(characterInfo.position);

            playerComponent.SetCharacterInfo(characterInfo);

            return playerObject;
        }

        //Send characters and make him to choose one of them
        public override async void OnServerConnect(NetworkConnection con)
        {
            PlayerSession data = (PlayerSession)con.authenticationData;

            Log(data.steamId, $"Connected from {con.address}");

            var www = Api.Request($"/users/{data.steamId}");

            await www.SendWebRequest();

            if (Api.GetResult(www) == null)
            {
                bool isCreated = await PlayerCharacterCreateRequest(con);
                if (!isCreated)
                {
                    WaitBeforeDisconnect(con);
                    return;
                }

                www = Api.Request($"/users/{data.steamId}");

                await www.SendWebRequest();
            }

            

            var wwwUpdate = Api.Request($"/session/{data.steamId}?status=1", ApiMethod.PUT);
            await wwwUpdate.SendWebRequest();
            Log(data.steamId, $"Setted online status");

            data.character = new CharacterInfo(Api.GetResult(www));

            con.authenticationData = data;

            if (isWhiteListEnabled)
            {
                www = Api.Request($"/whitelist/{data.steamId}");
                await www.SendWebRequest();
                if (Api.GetResult(www) == null)
                {
                    Log(data.steamId, $"Disconnect error: not in whitelist");
                    SendMessage(con, "You are not in whitelist", MessageType.Notification);
                    WaitBeforeDisconnect(con);
                    return;
                }
            }

            PlayerPlayRequest(con, data.character);
        }

        public override void OnServerAddPlayer(NetworkConnection conn) { }

        public override async void OnStopServer()
        {
            base.OnStopServer();
            await DeleteServerInDB();
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
#if !UNITY_EDITOR
            SteamManager.StartServer();
#endif
            CreateServerInDB();
        }

        private async Task UpdateCharacter(CharacterInfo info, Transform player)
        {
            var wwwUpdate = Api.Request($"/session/{info.steamId}?status=0", ApiMethod.PUT);
            await wwwUpdate.SendWebRequest();
            Log(info.steamId, $"Setted offline status");

            if (player == null)
            {
                Log(info.steamId, $"Disconnected without spawned character");
                return;
            }

            Vector3 pos;
            if (info.sceneChangeInfo != null)
            {
                pos = info.sceneChangeInfo.spawnPosition;
                info.location = info.sceneChangeInfo.sceneName;
            }
            else
            {
                pos = player.transform.position;
            }

            var www = Api.Request($"/users/{info.steamId}?" +
                //$"p_x={pos.x}&p_y={pos.y}&p_z={pos.z}&" +
                $"location={info.location}&" +
                $"health={(info.health > 0 ? info.health : 1000)}&" +
                $"personal_chests={PersonalChestInfo.ToJson(info.personalChests)}", ApiMethod.PUT);
            await www.SendWebRequest();

            Log(info.steamId, $"Setted disconnect position to {info.location} {pos}");
            www = Api.Request($"/positions/{info.steamId}?" +
                $"x={pos.x}&y={pos.y}&z={pos.z}", ApiMethod.PUT);
            await www.SendWebRequest();

            if (info.isSpawnPointChanged)
            {
                www = Api.Request($"/spawnpositions/{info.steamId}?x={info.spawnPoint.x}&y={info.spawnPoint.y}&z={info.spawnPoint.z}&location={info.spawnSceneName}", ApiMethod.PUT);
                await www.SendWebRequest();
                Log(info.steamId, $"Changed spawn point to {info.spawnSceneName} {info.spawnPoint}");
            }
        }

        private async Task UpdateCharacterData(PlayerSession info)
        {
            if (info.playerPrefab != null && info.playerPrefab.GetComponent<Player>() != null)
                info.character.health = info.playerPrefab.GetComponent<Player>().Health;
            await UpdateCharacter(info.character, info.playerPrefab);
            await UnloadPlayerContainers(info);
        }

        private async Task LoadPlayerContainers(PlayerSession info)
        {
            await ContainerData.LoadContainerAsync(info.character.inventoryId);
            await ContainerData.LoadContainerAsync(info.character.equipmentId);
            
            foreach (var chest in info.character.personalChests)
                await ContainerData.LoadContainerAsync(chest.ContainerId);

            Log(info.steamId, $"Containers loaded");
        }

        private async Task UnloadPlayerContainers(PlayerSession info)
        {
            await ContainerData.UpdateLoadedContainer(info.character.inventoryId);
            await ContainerData.UpdateLoadedContainer(info.character.equipmentId);

            foreach (var chest in info.character.personalChests)
                await ContainerData.UpdateLoadedContainer(chest.ContainerId);

            ContainerData.UnLoadContainer(info.character.equipmentId);
            ContainerData.UnLoadContainer(info.character.inventoryId);

            foreach (var chest in info.character.personalChests)
                ContainerData.UnLoadContainer(chest.ContainerId);

            Log(info.steamId, $"Containers unloaded and updated");
        }

        //Update character data on DB on disconneect
        public override async void OnServerDisconnect(NetworkConnection conn)
        {
            if (conn.authenticationData == null)
            {
                Log(0, $"Disconnect: auth data null");
                base.OnServerDisconnect(conn);
                if (connections.Contains(conn))
                    connections.Remove(conn);
                return;
            }

            PlayerSession data = (PlayerSession)conn.authenticationData;

            if (!data.isAuthorized)
            {
                Log(data.steamId, $"Disconnect: not authorized");
                connections.Remove(conn);
                base.OnServerDisconnect(conn);
                return;
            }

            if (data.character.steamId != 0 && !data.character.isOnlineOnAnotherServer)
                await UpdateCharacterData(data);
#if !UNITY_EDITOR
            SteamServer.EndSession(data.steamId);
#endif
            connections.Remove(conn);
            Log(data.steamId, $"Disconnect: default");
            base.OnServerDisconnect(conn);
        }

        private void ChangeScene(NetworkConnection con, string scene)
        {
            Debug.Log("Sended change scene");
            con.Send(new SceneMessage { sceneName = scene});
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

        public static void SendReconnectToThisServer(NetworkConnection con) 
        {
            SendMessage(con, $"{Instance.networkAddress}:{Instance.port}", MessageType.ServerRedirect);
        }

        public void RemoveUserFromConnections(ulong steamId) => connections.Remove(GetConnection(steamId));
        public void RemoveUserFromConnections(NetworkConnection con) => connections.Remove(con);

        public void AddConection(ulong steamId, NetworkConnection con) 
        {
            con.connectedTime = Time.time;
            con.steamId = steamId;
            connections.Add(con);
        }
        public List<NetworkConnection> GetConnections() => connections;
        public NetworkConnection GetConnection(ulong steamId) => connections.Find(con => con.steamId == steamId);
    }
}