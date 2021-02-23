using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEngine.UI;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Net.Mail;

public struct AuthRequestMessage : NetworkMessage
{
    public enum AuthType { Guest, Login, Register };
    public AuthType Type;
    public string Email;
    public string Password;

}

public struct AuthResponseMessage : NetworkMessage
{
    public bool Connected;
}

public struct PlayerConnection
{
    public int id;
    public List<Character> characters;
    public int selectedPlayer;
    public Transform playerPrefab;
}

public class ServerAuth : NetworkAuthenticator
{
    public override void OnStartServer()
    {
        GetComponent<MySqlServerConnection>().Init();
        NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
    }

    public override void OnServerAuthenticate(NetworkConnection conn)
    {
    }

    public bool IsEmailValid(string emailaddress)
    {
        try
        {
            MailAddress m = new MailAddress(emailaddress);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool IsRequestValid(AuthRequestMessage msg, ref int loginResponse)
    {
        if (!IsEmailValid(msg.Email))
            return false;
        switch (msg.Type)
        {
            case AuthRequestMessage.AuthType.Login:
                loginResponse = MySqlServerConnection.instance.Login(msg.Email, msg.Password);
                if (loginResponse != 0)
                    return true;
                break;
            case AuthRequestMessage.AuthType.Register:
                loginResponse = MySqlServerConnection.instance.Register(msg.Email, msg.Password);
                if (loginResponse != 0)
                    return true;
                break;
        }
        return false;
    }

    public void OnAuthRequestMessage(NetworkConnection conn, AuthRequestMessage msg)
    {
        int loginResponse = 0;

        bool result = IsRequestValid(msg, ref loginResponse);

        AuthResponseMessage authResponseMessage = new AuthResponseMessage();
        if (result)
            authResponseMessage.Connected = true;
        else
            authResponseMessage.Connected = false;
        conn.Send(authResponseMessage);

        if (result)
        {
            conn.authenticationData = new PlayerConnection
            {
                id = loginResponse,
                selectedPlayer = -1,
            };
            ServerAccept(conn);
        }
        else
            ServerReject(conn);
    }

    public override void OnClientAuthenticate(NetworkConnection conn)
    {
        
    }
}
