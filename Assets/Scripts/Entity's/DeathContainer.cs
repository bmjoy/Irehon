using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathContainer : Chest
{
    private void Start()
    {
        if (isServer)
        {
            OnContainerUpdate.AddListener(CheckIsContainerEmpty);
            StartCoroutine(SelfDestroyOnTime(240f));
        }
    }

    private void CheckIsContainerEmpty(Container container)
    {
        print(container.GetFilledSlotsCount());
        if (container.GetFilledSlotsCount() == 0)
            StartCoroutine(SelfDestroyOnTime(4f));
    }

    private IEnumerator SelfDestroyOnTime(float time)
    {
        yield return new WaitForSeconds(time);
        NetworkServer.Destroy(this.gameObject);
    }

    public override void SetChestId(int containerId) {
        base.SetChestId(containerId);
    }
    public void SetEquipment(Container equipment)
    {
        GetComponent<PlayerModelManager>()?.UpdateEquipmentContainer(equipment);
        SetEquipmentRpc(equipment);
    }

    [ClientRpc]
    private void SetEquipmentRpc(Container container)
    {
        GetComponent<PlayerModelManager>()?.UpdateEquipmentContainer(container);
    }

    private void OnDestroy()
    {
        OnContainerUpdate.RemoveListener(CheckIsContainerEmpty);
    }
}
