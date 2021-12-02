using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DisconnectButton : MonoBehaviour
{
    public void Disconnect()
    {
        print("Disconnect button event");
        NetworkClient.Disconnect();
    }
}
