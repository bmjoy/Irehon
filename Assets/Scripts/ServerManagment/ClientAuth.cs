using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class ClientAuth : NetworkAuthenticator
{
    [SerializeField]
    private TMPro.TMP_Text ResponseText;
    [SerializeField]
    private TMPro.TMP_InputField Email;
    [SerializeField]
    private TMPro.TMP_InputField Password;

    //private void Start()
    //{
    //    currentRequest.Login = "qwerty";
    //    currentRequest.Password = "1234";
    //    currentRequest.Type = AuthRequestMessage.AuthType.Login;
    //    GetComponent<NetworkManager>().StartClient();
    //}

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
        NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
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

    private void OnAuthResponseMessage(AuthResponseMessage msg)
    {
        if (msg.Connected)
            ClientAccept();
        else
        {
            ResponseText.text = msg.ResponseText;
            ClientReject();
        }
    }
}
