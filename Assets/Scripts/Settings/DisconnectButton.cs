using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DisconnectButton : MonoBehaviour
{
    public void Disconnect()
    {
        NetworkClient.Shutdown();
        LoginSceneUI.ShowPlayButton();
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }
}
