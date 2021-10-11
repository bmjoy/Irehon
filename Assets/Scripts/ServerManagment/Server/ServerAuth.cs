using Client;
using Mirror;
using Server;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Utils;
using Steamworks;

public struct AuthRequestMessage : NetworkMessage
{
    public ulong Id;
    public byte[] AuthData;
}

public struct PlayerConnectionInfo : NetworkMessage
{
    public ulong steamId;

    public CharacterInfo selectedCharacter;

    public Transform playerPrefab;

    public PlayerConnectionInfo(ulong steamId)
    {
        this.steamId = steamId;
        playerPrefab = null;
        selectedCharacter = new CharacterInfo();
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
        if (con.authenticationData != null || ServerManager.i.GetConnection(msg.Id) != null)
            return;

        if (msg.AuthData == null || msg.Id == 0)
            return;

        ServerManager.i.AddConection(msg.Id, con);

        SteamServer.BeginAuthSession(msg.AuthData, msg.Id);

        con.authenticationData = new PlayerConnectionInfo(msg.Id);
        SendAuthResult(con, true, "Connected");
    }

    private void OnAuthTicketResponse(SteamId user, SteamId owner, AuthResponse status)
    {
        if (ServerManager.i.GetConnection(user) == null)
            return;

        if (status == AuthResponse.OK)
            ServerAccept(ServerManager.i.GetConnection(user));
        else
            ServerReject(ServerManager.i.GetConnection(user));
    }

    public override void OnClientAuthenticate() {}

    public override void OnServerAuthenticate(NetworkConnection conn) { }
}
