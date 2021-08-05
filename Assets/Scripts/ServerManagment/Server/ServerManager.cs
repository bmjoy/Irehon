using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class ServerManager : NetworkManager
{
    public static ServerManager i;

    [SerializeField]
    private Vector3 characterSpawnPoint;
    private NetworkManager manager;
    [SerializeField]
    private GameObject mob;

    public override void Awake()
    {
        if (i != null && i != this)
            Destroy(gameObject);
        else
            i = this;
    }

    public override void Start()
    {
        manager = GetComponent<NetworkManager>();
        manager.clientLoadedScene = false;
        manager.StartServer();
        NetworkServer.RegisterHandler<CharacterSelection>(OnSelectedPlayerSlot, true);
        NetworkServer.RegisterHandler<CharacterCreate>(CharacterCreate, true);
    }

    //Insert data in tables and send it to player
    public void CharacterCreate(NetworkConnection con, CharacterCreate character)
    {
        var outer = Task.Factory.StartNew(() =>
        {
            PlayerConnection data = (PlayerConnection)con.authenticationData;
            int p_id = data.id;
            if (data.characters.Count > 2)
                return;

            if (!ServerAuth.IsLoginValid(character.NickName))
            {
                print("invalid login");
                SendMessage(con, "Invalid symbols in nickname", MessageType.Error);
                return;
            }

            if (MySql.Database.GetCharacterId(character.NickName) != 0)
            {
                print("Sended msg");
                SendMessage(con, "Nickname already in use", MessageType.Error);
                return;
            } 

            Character newCharacter = new Character
            {
                NickName = character.NickName,
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
    public void OnSelectedPlayerSlot(NetworkConnection con, CharacterSelection selection)
    {
        StartCoroutine(SpawnPlayer());
        IEnumerator SpawnPlayer()
        {
            if (selection.selectedSlot > 2)
                yield break;
            SceneMessage message = new SceneMessage
            {
                sceneName = "PvpScene",
            };
            con.Send(message);

            PlayerConnection data = (PlayerConnection)con.authenticationData;
            Character selectedCharacter = data.characters[selection.selectedSlot];

            int c_id = 0;

            var outer = Task.Factory.StartNew(() => c_id = MySql.Database.GetCharacterId(selectedCharacter.NickName));
            while (!outer.IsCompleted)
                yield return null;

            if (c_id == 0)
                yield break;

            data.selectedPlayer = c_id;

            GameObject playerObject = Instantiate(playerPrefab);
            playerObject.transform.position = selectedCharacter.position;

            Player playerComponent = playerObject.GetComponent<Player>();


            playerComponent.SetName(selectedCharacter.NickName);

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
        data.characters = MySql.Database.GetCharacters(data.id);
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
            if (data.selectedPlayer >= 0)
            {
                int c_id = data.selectedPlayer;
                Player player = data.playerPrefab.GetComponent<Player>();
                MySql.Database.UpdatePositionData(c_id, player.transform.position);
                MySql.Database.UpdateCharacterData(c_id, player.GetCharacterData());
            }
        });
        base.OnServerDisconnect(conn);
    }
}
