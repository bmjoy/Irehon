using DuloGames.UI;
using Irehon;
using Irehon.Entitys;
using System;
using UnityEngine;

public class SelectFraction : MonoBehaviour
{
    [SerializeField]
    private string startSceneName = "LoginScene";
    public void SelectFractionAndChangeScene(string fraction)
    {
        RegisterInfo info = new RegisterInfo((Fraction)Enum.Parse(typeof(Fraction), fraction));
        PlayerPrefs.SetString("Registration", info.ToJsonString());
        UILoadingOverlayManager.Instance.Create().LoadSceneAsync(this.startSceneName);
    }
}
