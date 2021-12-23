using Steamworks;
using System;
using UnityEngine;

namespace Irehon.Steam
{
    public class SteamManager : MonoBehaviour
    {
        public ulong idTest;
        private bool isServer;
        private bool isIntialized;
        private static SteamManager i;

        private void Awake()
        {
            if (i != null && i != this)
            {
                Destroy(this);
            }
            else
            {
                i = this;
            }

            DontDestroyOnLoad(this.gameObject);
        }

        public static void StartServer()
        {
            if (i.isIntialized)
            {
                return;
            }

            try
            {
                int port = int.Parse(Environment.GetEnvironmentVariable("SERVERPORT"));
                SteamServerInit init = new SteamServerInit()
                {
                    IpAddress = System.Net.IPAddress.Any,
                    Secure = true,
                    DedicatedServer = true,
                    GameDescription = "a",
                    GamePort = (ushort)(port + 1),
                    QueryPort = (ushort)(port + 2),
                    SteamPort = (ushort)(port + 3),
                    ModDir = "DedicatedTest",
                    VersionString = "1.0.0.0"
                };
                init.WithRandomSteamPort();

                i.isServer = true;
                i.isIntialized = true;
                SteamServer.OnSteamServerConnectFailure += (x, y) => Debug.Log($"Connection failed {x} {y}"); ;
                SteamServer.OnSteamServersDisconnected += x => Debug.Log($"Steam server disconnected {x}");
                SteamServer.OnSteamServersConnected += () => Debug.Log("Steam server connected");

                SteamServer.Init(1007, init, true);

                if (!SteamServer.LoggedOn)
                {
                    print("Steam server log on");
                    SteamServer.LogOnAnonymous();
                }

                Debug.Log("Steam server intaialized");
            }
            catch (Exception exception)
            {
                Debug.Log("Can't intialize steam client " + exception.ToString());
            }
        }

        public static bool StartClient()
        {
            if (i.isIntialized)
            {
                return true;
            }

            SteamClient.Init(1007, true);
            i.isServer = false;
            i.isIntialized = true;

            Debug.Log("Steam client intaialized");

            return true;
        }

        public static AuthTicket GetAuthTicket()
        {
            return SteamUser.GetAuthSessionTicket();
        }

        public static SteamId GetSteamId()
        {
            return SteamClient.SteamId;
        }

        private void OnDisable()
        {
            if (this.isServer && this.isIntialized)
            {
                SteamServer.Shutdown();
            }
            else if (this.isIntialized && !this.isServer)
            {
                SteamClient.Shutdown();
            }
            Debug.Log("Steam client shutdowned");
        }

        private void Update()
        {
            if (this.isServer && this.isIntialized)
            {
                SteamServer.RunCallbacks();
            }
            else if (this.isIntialized && !this.isServer)
            {
                SteamClient.RunCallbacks();
            }
        }
    }
}