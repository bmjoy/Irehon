using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEditor;
using UnityEngine.SceneManagement;

public class Server : NetworkManager
{
    NetworkManager manager;
    public override void Start()
    {
        manager = GetComponent<NetworkManager>();
        manager.StartServer();
        manager.ServerChangeScene("PvpScene");
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        print("Connected client from " + conn.address);
    }

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

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        print("Disconnected client " + conn.address);
    }
}
