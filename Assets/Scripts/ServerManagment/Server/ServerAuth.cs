using Client;
using Mirror;
using Server;
using SimpleJSON;
using Steamworks;
using System;
using System.Collections;
using UnityEngine;

public enum Fraction { None, North, South };
public struct RegisterInfo
{
    public Fraction fraction;
    public RegisterInfo(JSONNode node)
    {
        fraction = (Fraction)Enum.Parse(typeof(Fraction), node["fraction"]);
    }

    public RegisterInfo(Fraction fraction)
    {
        this.fraction = fraction;
    }

    public string ToJsonString()
    {
        JSONObject json = new JSONObject();
        json["fraction"] = fraction.ToString();
        return json.ToString();
    }
}

public struct AuthRequestMessage : NetworkMessage
{
    public ulong Id;
    public byte[] AuthData;
    public RegisterInfo registerInfo;
}

public struct PlayerConnectionInfo : NetworkMessage
{
    public ulong steamId;
    public bool isAuthorized;

    public AuthRequestMessage authInfo;

    public CharacterInfo character;

    public bool isSpawnPointChanged;
    public string sceneToChange;

    public Transform playerPrefab;

    public PlayerConnectionInfo(AuthRequestMessage authInfo)
    {
        this.steamId = authInfo.Id;
        isAuthorized = false;
        playerPrefab = null;
        character = new CharacterInfo();
        this.authInfo = authInfo;

        isSpawnPointChanged = false;
        sceneToChange = null;
    }
}

public class ServerAuth : NetworkAuthenticator
{
    public override void OnStartServer()
    {
        SteamServer.OnValidateAuthTicketResponse += OnAuthTicketResponse;
        NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
    }

    private void SendAuthResult(NetworkConnection con, bool isAuthenticated, string message)
    {
        if (isAuthenticated)
        {
            ServerManager.SendMessage(con, message, MessageType.AuthAccept);
            var oldData = (PlayerConnectionInfo)con.authenticationData;
            oldData.isAuthorized = true;
            con.authenticationData = oldData;
            ServerAccept(con);
        }
        else
        {
            ServerManager.SendMessage(con, message, MessageType.AuthReject);
            IEnumerator WaitBeforeDisconnect()
            {
                yield return new WaitForSeconds(0.1f);
                ServerReject(con);
            }
            StartCoroutine(WaitBeforeDisconnect());
        }
    }

    public void OnAuthRequestMessage(NetworkConnection con, AuthRequestMessage msg)
    {
#if UNITY_EDITOR
        if (ServerManager.i.GetConnection(msg.Id) != null)
            msg.Id++;
#endif
        if (con.authenticationData != null || ServerManager.i.GetConnection(msg.Id) != null)
        {
            return;
        }

        if (msg.AuthData == null || msg.Id == 0)
        {
            return;
        }

        ServerManager.i.AddConection(msg.Id, con);
#if UNITY_EDITOR
        con.authenticationData = new PlayerConnectionInfo(msg);
        SendAuthResult(con, true, "authorized");
#else
        SteamServer.BeginAuthSession(msg.AuthData, msg.Id);
        con.authenticationData = new PlayerConnectionInfo(msg);
#endif
    }

    private void OnAuthTicketResponse(SteamId user, SteamId owner, AuthResponse status)
    {
        if (ServerManager.i.GetConnection(user) == null)
            return;

        if (status == AuthResponse.OK)
            SendAuthResult(ServerManager.i.GetConnection(user), true, "authorized");
        else
            SendAuthResult(ServerManager.i.GetConnection(user), false, "steam auth error");
    }

    public override void OnClientAuthenticate() { }

    public override void OnServerAuthenticate(NetworkConnection conn) { }
}
