using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System;

public class TestUI : MonoBehaviour
{
    public InputField item;
    public InputField container;

    public void GiveItem()
    {
        StartCoroutine(ContainerData.GiveContainerItem(Convert.ToInt32(container.text), Convert.ToInt32(item.text)));
    }
}
