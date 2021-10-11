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
    private static SteamManager i;

    private void Awake()
    {
        if (i != null && i != this)
            Destroy(this);
        else
            i = this;
        try
        {
            SteamClient.Init(1759510);
            Debug.Log("Steam intaialized");
        }
        catch (Exception exception)
        {
            Debug.Log("Can't intialize steam client " + exception.ToString());
        }

        DontDestroyOnLoad(gameObject);
    }

    public static AuthTicket GetAuthTicket() => SteamUser.GetAuthSessionTicket();
    public static SteamId GetSteamId() => SteamClient.SteamId;
    private void OnDisable()
    {
        SteamClient.Shutdown();
        Debug.Log("Steam client shutdowned");
    }

    private void Update()
    {
        SteamClient.RunCallbacks();
    }
}
