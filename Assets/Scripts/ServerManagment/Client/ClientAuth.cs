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

    public override void OnClientAuthenticate()
    {
        NetworkClient.Send(currentRequest);
    }

    public override void OnServerAuthenticate(NetworkConnection conn)
    {
    }

    private AuthRequestMessage currentRequest;

    public override void OnStartClient()
    {
        ClientManager.OnGetServerMessage.AddListener(OnAuthResponseMessage);
    }

    public void RegisterButton()
    {
        currentRequest = new AuthRequestMessage()
        {
            Login = Email.text,
            Password = Password.text,
            Type = AuthRequestMessage.AuthType.Register
        };

        GetComponent<NetworkManager>().StartClient();
    }

    

    public void LoginButton()
    {
        currentRequest = new AuthRequestMessage()
        {
            Login = Email.text,
            Password = Password.text,
            Type = AuthRequestMessage.AuthType.Login
        };

        GetComponent<NetworkManager>().StartClient();
    }

    private void OnAuthResponseMessage(ServerMessage msg)
    {
        if (msg.messageType == MessageType.AuthAccept)
            ClientAccept();
        else if (msg.messageType == MessageType.AuthReject)
            ClientReject();
    }
}
