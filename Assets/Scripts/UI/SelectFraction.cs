using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using SimpleJSON;
using DuloGames.UI;
using System;

public class SelectFraction : MonoBehaviour
{
    [SerializeField]
    private string startSceneName = "LoginScene";
    public void SelectFractionAndChangeScene(string fraction)
    {
        RegisterInfo info = new RegisterInfo((Fraction)Enum.Parse(typeof(Fraction), fraction));
        PlayerPrefs.SetString("Registration", info.ToJsonString());
        UILoadingOverlayManager.Instance.Create().LoadSceneAsync(startSceneName);
    }
}
