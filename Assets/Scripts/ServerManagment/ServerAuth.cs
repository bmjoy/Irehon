using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEngine.UI;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

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
        print("auth");
    }

    public void OnAuthRequestMessage(NetworkConnection conn, AuthRequestMessage msg)
    {
        print(msg.Email + " passw = " + msg.Password + msg.Type);
        bool result = false;
        int loginResponse = 0;
        switch (msg.Type)
        {
            case AuthRequestMessage.AuthType.Login:
                loginResponse = MySqlServerConnection.instance.Login(msg.Email, msg.Password);
                if (loginResponse != 0)
                    result = true;
                break;
            case AuthRequestMessage.AuthType.Register:
                MySqlServerConnection.instance.Register(msg.Email, msg.Password);
                result = true;
                break;
        }

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
                id = loginResponse
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
