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

    public void CharacterCreate(NetworkConnection con, CharacterCreate character)
    {
        PlayerConnection data = (PlayerConnection)con.authenticationData;
        int p_id = data.id;
        if (data.characters.Count > 2)
            return;

        Character newCharacter = new Character
        {
            Class = character.Class,
            NickName = character.NickName,
            position = characterSpawnPoint
        };

        MySqlServerConnection.instance.CreateNewCharacter(p_id, newCharacter);

        SendCharacterListToPlayer(con);
    }

    public void OnSelectedPlayerSlot(NetworkConnection con, CharacterSelection selection)
    {
        SceneMessage message = new SceneMessage
        {
            sceneName = "PvpScene",
        };
        con.Send(message);

        PlayerConnection data = (PlayerConnection)con.authenticationData;
        Character selectedCharacter = data.characters[selection.selectedSlot];

        //тут нужно будет искать по классам
        GameObject playerObject = Instantiate(spawnPrefabs.Find(x => x.name == "ArcherPlayer"));

        playerObject.transform.position = selectedCharacter.position;

        data.playerPrefab = playerObject.transform;

        data.selectedPlayer = MySqlServerConnection.instance.GetCharacterId(selectedCharacter.NickName);

        con.authenticationData = data;

        NetworkServer.AddPlayerForConnection(con, playerObject);
    }

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

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        PlayerConnection data = (PlayerConnection)conn.authenticationData;
        //print(data.selectedPlayer);
        if (data.selectedPlayer >= 0)
            MySqlServerConnection.instance.UpdatePositionData(data.selectedPlayer, data.playerPrefab.position);
        base.OnClientDisconnect(conn);
        print("Disconnected client " + conn.address);
    }
}
