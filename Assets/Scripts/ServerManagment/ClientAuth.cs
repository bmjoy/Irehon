using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEngine.UI;

public class ClientAuth : NetworkAuthenticator
{
    [SerializeField]
    private Text Email;
    [SerializeField]
    private Text Password;

    private struct AuthRequestMessage : NetworkMessage
    {
        public enum AuthType { Guest, Login, Register };
        public AuthType Type;
        public string Email;
        public string Password;

    }

    private AuthRequestMessage currentRequest;

    public struct AuthResponseMessage : NetworkMessage
    {
        public bool Connected;
    }


    public override void OnStartClient()
    {
        NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
    }

    public override void OnServerAuthenticate(NetworkConnection conn) {}

    public override void OnClientAuthenticate(NetworkConnection conn)
    {
        NetworkClient.Send(currentRequest);
        print("Sended request");
    }

    public void RegisterButton()
    {
        currentRequest = new AuthRequestMessage()
        {
            Email = Email.text,
            Password = Password.text,
            Type = AuthRequestMessage.AuthType.Register
        };

        GetComponent<NetworkManager>().StartClient();
    }

    public void LoginButton()
    {
        currentRequest = new AuthRequestMessage()
        {
            Email = Email.text,
            Password = Password.text,
            Type = AuthRequestMessage.AuthType.Login
        };

        GetComponent<NetworkManager>().StartClient();
    }

    private void OnAuthResponseMessage(NetworkConnection conn, AuthResponseMessage msg)
    {
        if (msg.Connected)
            ClientAccept(conn);
        else
            ClientReject(conn);
    }
}
