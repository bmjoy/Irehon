using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System;
using UnityEngine.Networking;

public class TestUI : MonoBehaviour
{
    public InputField item;
    public InputField container;

    [SerializeField]
    private bool isWebTest;
    private async void Start()
    {
        if (isWebTest)
        {
            print(Environment.GetEnvironmentVariable("asdagasfgafsgasf asfg"));
            Environment.SetEnvironmentVariable("Agfs", "fgf");
        }
    }

    public void GiveItem()
    {
        StartCoroutine(ContainerData.GiveContainerItem(Convert.ToInt32(container.text), Convert.ToInt32(item.text)));
    }
}
