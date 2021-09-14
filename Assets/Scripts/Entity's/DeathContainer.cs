using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathContainer : Chest
{
    public override void SetChestId(int containerId) {
        base.SetChestId(containerId);
        
    }
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
