using Client;
using Mirror;
using Server;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

public struct AuthRequestMessage : NetworkMessage
{
    public enum AuthType { Guest, Login, Register };
    public AuthType Type;
    public string Login;
    public string Password;

}

public struct PlayerConnection
{
    public int playerId;
    public List<Character> characters;
    public int characterId;
    public Transform playerPrefab;
}

public class ServerAuth : NetworkAuthenticator
{
    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
    }

    public override void OnServerAuthenticate(NetworkConnection conn)
    {
    }

    public static int GetPassword(string password) => password.GetHashCode();

    public static bool IsLoginValid(string login)
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
                loginResponse = MySql.Database.Login(msg.Login, passwordHash);
                if (loginResponse > 0)
                {
                    response = "Succesful";
                    return true;
                }
                response = "Login or Password incorrect";
                return false;
            case AuthRequestMessage.AuthType.Register:
                loginResponse = MySql.Database.Register(msg.Login, passwordHash);
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

            if (result)
                ServerManager.SendMessage(conn, responseText, MessageType.AuthAccept);
            else
                ServerManager.SendMessage(conn, responseText, MessageType.AuthReject);

            if (result)
            {
                conn.authenticationData = new PlayerConnection
                {
                    playerId = loginResponse,
                    characterId = -1,
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
