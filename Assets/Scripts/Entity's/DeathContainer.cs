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
            Container.OnContainerUpdate += CheckIsContainerEmpty;
            StartCoroutine(SelfDestroyOnTime(240f));
        }
    }

    public void CheckIsContainerEmpty(Container container)
    {
        if (container.GetFilledSlotsCount() == 0)
            StartCoroutine(SelfDestroyOnTime(4f));
    }

    public void AttachMultipleContainers(List<Container> otherContainers)
    {
        Container newContainer = Container.MoveAllItemsInNewContainer(otherContainers);

        SetChestContainer(newContainer);
    }

    private IEnumerator SelfDestroyOnTime(float time)
    {
        yield return new WaitForSeconds(time);
        NetworkServer.Destroy(this.gameObject);
    }

    protected override void OnContainerSet()
    {
        Container.OnContainerUpdate += CheckIsContainerEmpty;
        CheckIsContainerEmpty(Container);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Container.OnContainerUpdate -= CheckIsContainerEmpty;
    }
}
