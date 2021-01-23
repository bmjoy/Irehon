using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    Server serverCore;

    private void Start()
    {
        serverCore = GetComponent<Server>();

        serverCore.Init();
    }
}
