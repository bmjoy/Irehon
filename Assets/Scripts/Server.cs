using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Server : NetworkManager
{
    NetworkManager manager;
    public void Init()
    {
        manager = GetComponent<NetworkManager>();
#if !UNITY_EDITOR
        manager.StartServer();
#else
        manager.StartClient();
#endif
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log("Connected client from " + conn.address);
        Console.WriteLine("Connected client from " + conn.address);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        Debug.Log("Stopped server");
        Console.WriteLine("Stopped server");
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("Started server");
        Console.WriteLine("Started  server");
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        Debug.Log("Disconnected client " + conn.address);
        Console.WriteLine("Disconnected client " + conn.address);
    }
}
