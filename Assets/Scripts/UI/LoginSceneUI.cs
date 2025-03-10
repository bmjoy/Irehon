﻿using Irehon.Client;
using UnityEngine;

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
            this.playButton.gameObject.SetActive(true);
        }
        else
        {
            this.loadingBar.gameObject.SetActive(true);
        }
    }

    public static void ShowPlayButton()
    {
        isPlayButtonShowable = true;
    }

    public static void HidePlayButton()
    {
        isPlayButtonShowable = false;
        if (i != null && i.playButton != null)
        {
            i.playButton?.SetActive(false);
        }
    }

    public static void ShowLoadingBar()
    {
        if (i != null && i.loadingBar != null)
        {
            i.loadingBar?.SetActive(true);
        }
    }

    public void Play()
    {
        ClientManager.i.GetComponent<ClientAuth>().PlayButton();
    }

}
