using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class PlayerContainersUI : NetworkBehaviour
{
    private Dictionary<int, ContainerType> containersId = new Dictionary<int, ContainerType>();

    public void SendContainerInfo(int containerId, Container container)
    {

    }

    public void IntializeContainerType(int id, ContainerType type)
    {
        containersId[id] = type;
    }
}
