using Irehon;
using Irehon.Client;
using Mirror;
using Steamworks;
using System;
using System.Collections;
using UnityEngine;

public class ServerAuth : NetworkAuthenticator
{
    public override void OnStartServer()
    { 
        SteamServer.OnValidateAuthTicketResponse += OnAuthTicketResponse;
        NetworkServer.RegisterHandler<AuthInfo>(OnAuthRequestMessage, false);
        InvokeRepeating(nameof(AuthTimeoutChecker), 2, 2);
    }

    private void AuthTimeoutChecker()
    {
        foreach (NetworkConnection con in ServerManager.Instance.GetConnections())
        {
            if (!con.isAuthenticated)
            {
                if (Time.time - con.connectedTime > 3.5f)
                    SendAuthResult(con, false, "Steam auth request timeout");
            }
        }
    }

    private void SendAuthResult(NetworkConnection con, bool isAuthenticated, string message)
    {
        if (isAuthenticated)
        {
            ServerManager.SendMessage(con, message, MessageType.AuthAccept);
            var oldData = (Irehon.PlayerSession)con.authenticationData;
            oldData.isAuthorized = true;
            con.authenticationData = oldData;
            ServerAccept(con);
        }
        else
        {
            ServerManager.SendMessage(con, message, MessageType.AuthReject);
            IEnumerator WaitBeforeDisconnect()
            {
                yield return new WaitForSeconds(0.2f);
                ServerReject(con);
            }
            StartCoroutine(WaitBeforeDisconnect());
        }
    }

    public void OnAuthRequestMessage(NetworkConnection con, AuthInfo msg)
    {
        Debug.Log($"{DateTime.Now} [[{msg.Id} {con.address}]] connection request");
#if UNITY_EDITOR
        if (ServerManager.Instance.GetConnection(msg.Id) != null)
            msg.Id++;
#endif
        if (msg.version == null || msg.version != Application.version)
        {
            Debug.Log($"[[{msg.Id} {con.address}]] mismatch client version");
            SendAuthResult(con, false, "your client outdated, please update game");
            return;
        }

        if (con.authenticationData != null || ServerManager.Instance.GetConnection(msg.Id) != null)
        {
            Debug.Log($"[[{msg.Id} {con.address}]] second connect attemp");
            SendAuthResult(con, false, "already connected");
            return;
        }

        if (msg.AuthData == null || msg.Id == 0)
        {
            Debug.Log($"[[{msg.Id} {con.address}]] empty auth data");
            SendAuthResult(con, false, "null auth data");
            return;
        }

        ServerManager.Instance.AddConection(msg.Id, con);
#if UNITY_EDITOR
        con.authenticationData = new PlayerSession(msg);
        SendAuthResult(con, true, "authorized");
#else
        SteamServer.BeginAuthSession(msg.AuthData, msg.Id);
        con.authenticationData = new PlayerSession(msg);
#endif
    }

    private void OnAuthTicketResponse(SteamId user, SteamId owner, AuthResponse status)
    {
        if (ServerManager.Instance.GetConnection(user) == null)
            return;

        if (status == AuthResponse.OK)
            SendAuthResult(ServerManager.Instance.GetConnection(user), true, "authorized");
        else
            SendAuthResult(ServerManager.Instance.GetConnection(user), false, "steam auth error");
    }

    public override void OnClientAuthenticate() { }

    public override void OnServerAuthenticate(NetworkConnection conn) { }
}
