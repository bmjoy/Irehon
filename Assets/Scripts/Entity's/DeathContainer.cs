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

    public void CheckIsContainerEmpty(Container container)
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

    protected override void OnDestroy()
    {
        base.OnDestroy();
        OnContainerUpdate.RemoveListener(CheckIsContainerEmpty);
    }
}
