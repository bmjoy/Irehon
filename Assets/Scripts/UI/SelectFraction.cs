using DuloGames.UI;
using Irehon;
using Irehon.Entitys;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SelectFraction : MonoBehaviour
{
    [SerializeField]
    private string startSceneName = "LoginScene";

    [SerializeField]
    private Color northColor;
    [SerializeField]
    private Color southColor;

    [SerializeField]
    private Image createButtonBG;
    [SerializeField]
    private Text fractionDescription;

    [SerializeField]
    private string northDecsription, southDescription;

    private Fraction selectedFraction = Fraction.None;

    public void SetSelectedFraction(string fraction)
    {
        selectedFraction = (Fraction)Enum.Parse(typeof(Fraction), fraction);
        
        if (selectedFraction == Fraction.North)
        {
            createButtonBG.color = northColor;
            fractionDescription.text = northDecsription;
        }
        else if (selectedFraction == Fraction.South)
        {
            createButtonBG.color = southColor;
            fractionDescription.text = southDescription;
        }
    }

    public void ChangeScene()
    {
        if (selectedFraction == Fraction.None)
            return;

        RegisterInfo info = new RegisterInfo(selectedFraction);
        PlayerPrefs.SetString("Registration", info.ToJsonString());
        Irehon.Client.ClientAuth.isShouldAutoLoad = true;
        LoginSceneUI.HidePlayButton();
        UILoadingOverlayManager.Instance.Create().LoadSceneAsync(this.startSceneName);
    }
}
