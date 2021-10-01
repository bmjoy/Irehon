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

public struct AuthRequestMessage : NetworkMessage
{
    public enum AuthType { Guest, Login, Register };
    public AuthType Type;
    public string Login;
    public string Password;

}

public struct PlayerConnectionInfo : NetworkMessage
{
    public int playerId;
    public Character[] characters;

    public CharacterInfo selectedCharacter;

    public Transform playerPrefab;

    public PlayerConnectionInfo(int playerId)
    {
        this.playerId = playerId;
        characters = new Character[0];
        playerPrefab = null;
        selectedCharacter = new CharacterInfo();
    }
    public PlayerConnectionInfo(JSONNode node)
    {
        playerId = node["id"].AsInt;

        List<Character> characters = new List<Character>();

        foreach (JSONNode character in node["characters"])
            characters.Add(new Character(character));
        
        this.characters = characters.ToArray();

        playerPrefab = null;
        
        selectedCharacter = new CharacterInfo();
    }

}

public class ServerAuth : NetworkAuthenticator
{
    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
    }

    public static int GetPassword(string password) => password.GetHashCode();

    public static bool IsPasswordValid(string password) => password.Length > 5;

    public static bool IsLoginSymbolsValid(string login) => Regex.IsMatch(login, @"^[a-zA-Z0-9_]+$");
    public static bool IsLengthLoginValid(string login) => (login.Length < 15 && login.Length > 2);

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
    
    private void Register(NetworkConnection con, AuthRequestMessage msg)
    {
        StartCoroutine(LoginCoroutine());
        IEnumerator LoginCoroutine()
        {
            int password = msg.Password.GetStableHashCode();

            var www = Api.Request($"/users/?login={msg.Login}&password={password}", ApiMethod.POST);
            yield return www.SendWebRequest();
            var result = Api.GetResult(www);
            
            if (result != null)
            {
                Debug.Log($"{result}, {result["id"].AsInt}");
                con.authenticationData = new PlayerConnectionInfo(result["id"].AsInt);
                SendAuthResult(con, true, "Succesful");
            }
            else
                SendAuthResult(con, false, "User with this login exist");
        }
    }

    private void Login(NetworkConnection con, AuthRequestMessage msg)
    {
        StartCoroutine(LoginCoroutine());
        IEnumerator LoginCoroutine()
        {
            int password = msg.Password.GetStableHashCode();
            
            var www = Api.Request($"/auth/?login={msg.Login}&password={password}");
            yield return www.SendWebRequest();
            var result = Api.GetResult(www);

            if (result != null)
            {
                var data = new PlayerConnectionInfo(result);
                
                if (ServerManager.ConnectedPlayers.Contains(data.playerId))
                {
                    SendAuthResult(con, false, "Already connected");
                    yield break;
                }

                con.authenticationData = data;

                SendAuthResult(con, true, "Succesful");
            }
            else
                SendAuthResult(con, false, "Login or Password incorrect");
        }
    }

    public void OnAuthRequestMessage(NetworkConnection con, AuthRequestMessage msg)
    {
        if (!IsLoginSymbolsValid(msg.Login))
            SendAuthResult(con, false, "Login can contain only letters or digits");
        else if (!IsLengthLoginValid(msg.Login))
            SendAuthResult(con, false, "Invalid login length");
        else if (!IsPasswordValid(msg.Password))
            SendAuthResult(con, false, "Password too short");
        else if (msg.Type == AuthRequestMessage.AuthType.Login)
            Login(con, msg);
        else if (msg.Type == AuthRequestMessage.AuthType.Register)
            Register(con, msg);
    }

    public override void OnClientAuthenticate() {}

    public override void OnServerAuthenticate(NetworkConnection conn) { }
}
