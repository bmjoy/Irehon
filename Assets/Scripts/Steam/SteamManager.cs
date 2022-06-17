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

        public static bool StartClient()
        {
            if (i.isIntialized)
            {
                return true;
            }

            SteamClient.Init(1759510, true);
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