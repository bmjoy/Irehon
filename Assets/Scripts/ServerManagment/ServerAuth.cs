using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEngine.UI;
using MySql.Data.MySqlClient;

public class ServerAuth : NetworkAuthenticator
{
    public struct AuthRequestMessage : NetworkMessage
    {
        public enum AuthType { Guest, Login, Register };
        public AuthType Type;
        public string Email;
        public string Password;

    }

    public MySqlConnection connection;

    public struct AuthResponseMessage : NetworkMessage
    {
        public bool Connected;
    }

    public override void OnStartServer()
    {
        GetComponent<MySqlServerConnection>().Init();
    }

    public override void OnServerAuthenticate(NetworkConnection conn)
    {
    }

    public void OnAuthRequestMessage(NetworkConnection conn, AuthRequestMessage msg)
    {
        print(msg.Email + " passw = " + msg.Password + msg.Type);
        bool result = false;
        switch (msg.Type)
        {
            case AuthRequestMessage.AuthType.Login:
                string loginResponse = MySqlServerConnection.instance.Login(msg.Email, msg.Password);
                if (loginResponse != null && loginResponse != "")
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
            ServerAccept(conn);
        else
            ServerReject(conn);
    }

    public override void OnClientAuthenticate(NetworkConnection conn)
    {
    }
}
