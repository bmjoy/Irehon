using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoginSceneUI : MonoBehaviour
{
    private static LoginSceneUI i;

    private static bool isPlayButtonShowable = true;
    [SerializeField]
    private GameObject playButton;
    [SerializeField]
    private GameObject loadingBar;
    private void Awake()
    {
        i = this;
        if (isPlayButtonShowable)
        {
            playButton.gameObject.SetActive(true);
        }
        else
            loadingBar.gameObject.SetActive(true);
    }

    public static void ShowPlayButton()
    {
        isPlayButtonShowable = true;
    }

    public static void HidePlayButton()
    {
        isPlayButtonShowable = false;
        if (i != null && i.playButton != null)
            i.playButton?.SetActive(false);
    }

    public void Play()
    {
        Client.ClientManager.i.GetComponent<Client.ClientAuth>().PlayButton();
    }
    
}
