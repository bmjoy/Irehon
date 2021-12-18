using Mirror;
using UnityEngine;

public class DisconnectButton : MonoBehaviour
{
    public void Disconnect()
    {
        NetworkClient.Shutdown();
        LoginSceneUI.ShowPlayButton();
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }
}
