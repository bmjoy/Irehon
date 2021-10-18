using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

public class SteamManager : MonoBehaviour
{
    public ulong idTest;
    private bool isServer;
    private bool isIntialized;
    private static SteamManager i;

    private void Awake()
    {
        if (i != null && i != this)
            Destroy(this);
        else
            i = this;

        DontDestroyOnLoad(gameObject);
    }

    public static void StartServer()
    {
        if (i.isIntialized)
            return;

        try
        {
            SteamServerInit init = new SteamServerInit()
            {
                IpAddress = System.Net.IPAddress.Any,
                Secure = true,
                DedicatedServer = true,
                GameDescription = "a",
                GamePort = (ushort)(ushort.Parse(Environment.GetEnvironmentVariable("PORT")) + 1),
                QueryPort = (ushort)(ushort.Parse(Environment.GetEnvironmentVariable("PORT")) + 2),
                SteamPort = (ushort)(ushort.Parse(Environment.GetEnvironmentVariable("PORT")) + 3),
                ModDir = "DedicatedTest",
                VersionString = "1.0.0.0"
            };
            init.WithRandomSteamPort();

            i.isServer = true;
            SteamServer.OnSteamServerConnectFailure += (x,y) => Debug.Log($"Connection failed {x} {y}"); ;
            SteamServer.OnSteamServersDisconnected += x => Debug.Log($"Steam server disconnected {x}");
            SteamServer.OnSteamServersConnected += () => Debug.Log("Steam server connected");
            SteamServer.Init(1007, init, true);
            SteamServer.LogOnAnonymous();
            Debug.Log("Steam server intaialized");
        }
        catch (Exception exception)
        {
            Debug.Log("Can't intialize steam client " + exception.ToString());
        }
    }

    public static void StartClient()
    {
        if (i.isIntialized)
            return;
        try
        {
            i.isServer = false;
            SteamClient.Init(1007, true);
            Debug.Log("Steam client intaialized");
        }
        catch (Exception exception)
        {
            Debug.Log("Can't intialize steam client " + exception.ToString());
        }
    }

    public static AuthTicket GetAuthTicket() => SteamUser.GetAuthSessionTicket();
    public static SteamId GetSteamId() => SteamClient.SteamId;
    private void OnDisable()
    {
        if (isServer && isIntialized)
            SteamServer.Shutdown();
        else if (isIntialized && !isServer)
            SteamClient.Shutdown();
        Debug.Log("Steam client shutdowned");
    }

    private void Update()
    {
        if(isServer && isIntialized)
            SteamServer.RunCallbacks();
        else if (isIntialized && !isServer)
            SteamClient.RunCallbacks();
    }
}
