using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEditor;
using UnityEngine.SceneManagement;

public class Server : NetworkManager
{
    private NetworkManager manager;
    private Vector3 characterSpawnPoint;

    public override void Start()
    {
        manager = GetComponent<NetworkManager>();
        manager.clientLoadedScene = false;
        manager.StartServer();
        manager.ServerChangeScene("PvpScene");
        NetworkServer.RegisterHandler<CharacterSelection>(OnSelectedPlayerSlot, true);
        NetworkServer.RegisterHandler<CharacterCreate>(CharacterCreate, true);
        characterSpawnPoint = new Vector3(-2, 2, -73);
    }

    //Insert data in tables and send it to player
    public void CharacterCreate(NetworkConnection con, CharacterCreate character)
    {
        PlayerConnection data = (PlayerConnection)con.authenticationData;
        int p_id = data.id;
        if (data.characters.Count > 2)
            return;

        Character newCharacter = new Character
        {
            NickName = character.NickName,
            position = characterSpawnPoint,
        };

        MySqlServerConnection.instance.CreateNewCharacter(p_id, newCharacter);

        SendCharacterListToPlayer(con);
    }

    //Spawn character on player selected slot 
    public void OnSelectedPlayerSlot(NetworkConnection con, CharacterSelection selection)
    {
        SceneMessage message = new SceneMessage
        {
            sceneName = "PvpScene",
        };
        con.Send(message);

        PlayerConnection data = (PlayerConnection)con.authenticationData;
        Character selectedCharacter = data.characters[selection.selectedSlot];

        int c_id = MySqlServerConnection.instance.GetCharacterId(selectedCharacter.NickName);

        data.selectedPlayer = c_id;

        GameObject playerObject = Instantiate(spawnPrefabs.Find(x => x.name == "ArcherPlayer"));

        playerObject.transform.position = selectedCharacter.position;

        Player playerComponent = playerObject.GetComponent<Player>();

        playerComponent.SetName(selectedCharacter.NickName);



        data.playerPrefab = playerObject.transform;

        NetworkServer.AddPlayerForConnection(con, playerObject);

        CharacterData characterData = MySqlServerConnection.instance.GetCharacterData(c_id);
        playerComponent.SetCharacterData(characterData);
        
        con.authenticationData = data;
    }

    //Send characters and make him to choose one of them
    public override void OnServerConnect(NetworkConnection conn)
    {
        print("Connected client from " + conn.address);
        SceneMessage message = new SceneMessage
        {
            sceneName = "CharacterSelection",
        };
        conn.Send(message);

        SendCharacterListToPlayer(conn);
    }

    //Character list to choose on of them
    private void SendCharacterListToPlayer(NetworkConnection con)
    {
        PlayerConnection data = (PlayerConnection)con.authenticationData;
        data.characters = MySqlServerConnection.instance.GetCharacters(data.id);
        foreach (Character character in data.characters)
            con.Send(character);
        con.authenticationData = data;
    }

    public override void OnServerAddPlayer(NetworkConnection conn) { }

    public override void OnStopServer()
    {
        base.OnStopServer();
        print("Stopped server");
    }

    public override void OnStartServer()    
    {
        base.OnStartServer();
        print("Started server");
    }

    //Update character data on DB on disconneect
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        PlayerConnection data = (PlayerConnection)conn.authenticationData;
        if (data.selectedPlayer >= 0)
        {
            int c_id = data.selectedPlayer;
            Player player = data.playerPrefab.GetComponent<Player>();
            MySqlServerConnection.instance.UpdatePositionData(c_id, player.transform.position);
            MySqlServerConnection.instance.UpdateCharacterData(c_id, player.GetCharacterData());
        }
        base.OnClientDisconnect(conn);
    }
}
