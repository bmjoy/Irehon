using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathContainer : NetworkBehaviour
{
    public void SetEquipment(Container equipment)
    {
        GetComponent<PlayerModelManager>().UpdateEquipmentContainer(equipment);
        SetEquipmentRpc(equipment);
    }

    [ClientRpc]
    private void SetEquipmentRpc(Container container)
    {
        GetComponent<PlayerModelManager>().UpdateEquipmentContainer(container);
    }
}
