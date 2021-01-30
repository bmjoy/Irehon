using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugNetwork : MonoBehaviour
{
    public static DebugNetwork instance;

    [SerializeField]
    private Text ping;

    private void Awake()
    {
        instance = this;
    }

    public void ShowPing(int ms)
    {
        ping.text = "Ping = " + ms.ToString();
    }
}
