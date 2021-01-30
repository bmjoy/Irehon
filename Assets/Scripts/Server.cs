using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEditor;

public class Server : NetworkManager
{
    NetworkManager manager;
    public override void Start()
    {
        manager = GetComponent<NetworkManager>();
        if (GetComponent<NetworkManagerHUD>() != null)
            return;
#if UNITY_EDITOR
        manager.networkAddress = "localhost";
        manager.StartHost();
#elif UNITY_WEBGL
        manager.StopClient();
#endif
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        print("Connected client from " + conn.address);
        Console.WriteLine("Connected client from " + conn.address);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        print("Stopped server");
        Console.WriteLine("Stopped server");
    }

    public override void OnStartServer()    
    {
        base.OnStartServer();
        print("Started server");
        Console.WriteLine("Started  server");
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        print("Disconnected client " + conn.address);
        Console.WriteLine("Disconnected client " + conn.address);
    }
}
