using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private string startClassName;

    private PlayerController controll;
    private void Start()
    {
        controll = GetComponent<PlayerController>();
        controll.Init();
        controll.EnableClass(startClassName);
    }
}
