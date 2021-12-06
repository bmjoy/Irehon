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
        if (container.GetFilledSlotsCount() == 0)
            StartCoroutine(SelfDestroyOnTime(4f));
    }

    public async void AttachMultipleContainers(List<int> otherContainers)
    {
        int newContainer = await ContainerData.MergeContainers(otherContainers);

        SetChestId(newContainer);
        CheckIsContainerEmpty(ContainerData.LoadedContainers[newContainer]);
    }

    private IEnumerator SelfDestroyOnTime(float time)
    {
        yield return new WaitForSeconds(time);
        NetworkServer.Destroy(this.gameObject);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        OnContainerUpdate.RemoveListener(CheckIsContainerEmpty);
    }
}
