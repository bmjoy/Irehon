using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEngine.UI;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Collections;
using System.Threading.Tasks;

public struct AuthRequestMessage : NetworkMessage
{
    public enum AuthType { Guest, Login, Register };
    public AuthType Type;
    public string Login;
    public string Password;

}

public struct AuthResponseMessage : NetworkMessage
{
    public bool Connected;
    public string ResponseText;
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
        GetComponent<MySql.Connection>().Init();
        NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
    }

    public override void OnServerAuthenticate(NetworkConnection conn)
    {
    }

    public int GetPassword(string password) => password.GetHashCode();

    public bool IsLoginValid(string login)
    {
        if (login.Length > 20)
            return false;
        return Regex.IsMatch(login, @"^[a-zA-Z0-9_]+$");
    }

    public bool IsRequestValid(AuthRequestMessage msg, ref int loginResponse, out string response)
    {
        if (!IsLoginValid(msg.Login))
        {
            response = "Login can contain only letters or digits";
            return false;
        }
        int passwordHash = msg.Password.GetStableHashCode();
        switch (msg.Type)
        {
            case AuthRequestMessage.AuthType.Login:
                loginResponse = MySql.Database.instance.Login(msg.Login, passwordHash);
                if (loginResponse > 0)
                {
                    response = "Succesful";
                    return true;
                }
                response = "Login or Password incorrect";
                return false;
            case AuthRequestMessage.AuthType.Register:
                loginResponse = MySql.Database.instance.Register(msg.Login, passwordHash);
                if (loginResponse > 0)
                {
                    response = "Succesful";
                    return true;
                }
                response = "User with this login exist";
                return false;
        }
        response = "Invalid auth request";
        return false;
    }

    public void OnAuthRequestMessage(NetworkConnection conn, AuthRequestMessage msg)
    {
        var outer = Task.Factory.StartNew(() =>
        {
            int loginResponse = 0;
            string responseText;

            bool result = IsRequestValid(msg, ref loginResponse, out responseText);

            AuthResponseMessage authResponseMessage = new AuthResponseMessage();
            authResponseMessage.ResponseText = responseText;
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
            {
                IEnumerator WaitBeforeDisconnect()
                {
                    yield return new WaitForSeconds(0.1f);
                    ServerReject(conn);
                }
                StartCoroutine(WaitBeforeDisconnect());
            }
        });
    }

    public override void OnClientAuthenticate()
    {
        
    }
}
