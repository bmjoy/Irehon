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

    public override void Start()
    {
        manager = GetComponent<NetworkManager>();
        manager.clientLoadedScene = false;
        manager.StartServer();
        manager.ServerChangeScene("PvpScene");
        NetworkServer.RegisterHandler<CharacterSelection>(OnSelectedPlayerSlot, true);
    }

    public void OnSelectedPlayerSlot(NetworkConnection con, CharacterSelection selection)
    {
        SceneMessage message = new SceneMessage
        {
            sceneName = "PvpScene",
        };
        con.Send(message);

        if (con.authenticationData != null)
            print(con.authenticationData.ToString());
        else 
            print("null");

        PlayerConnection data = (PlayerConnection)con.authenticationData;
        Character selectedCharacter = data.characters[selection.selectedSlot];

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
        PlayerConnection data = (PlayerConnection)conn.authenticationData;
        data.characters = MySqlServerConnection.instance.GetCharacters(data.id);
        foreach (Character character in data.characters)
        {
            conn.Send(character);
        }
        conn.authenticationData = data;
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
        MySqlServerConnection.instance.UpdatePositionData(data.selectedPlayer, data.playerPrefab.position);
        base.OnClientDisconnect(conn);
        print("Disconnected client " + conn.address);
    }
}
