using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class ClientAuth : NetworkAuthenticator
{
    [SerializeField]
    private TMPro.TMP_InputField Email;
    [SerializeField]
    private TMPro.TMP_InputField Password;

    public override void OnClientAuthenticate(NetworkConnection conn)
    {
        NetworkClient.Send(currentRequest);
        print("Sended request");
    }

    public override void OnServerAuthenticate(NetworkConnection conn)
    {
        
    }

    private AuthRequestMessage currentRequest;

    public override void OnStartClient()
    {
        NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
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
        print("response = " + msg.Connected);
        if (msg.Connected)
            ClientAccept(conn);
        else
            ClientReject(conn);
    }
}
