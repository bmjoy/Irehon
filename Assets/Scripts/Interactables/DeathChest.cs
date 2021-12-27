using Irehon.Interactable;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathChest : Chest
{
    private void Start()
    {
        if (this.isServer)
        {
            this.Container.ContainerSlotsChanged += this.CheckIsContainerEmpty;
            this.StartCoroutine(this.SelfDestroyOnTime(240f));
        }
    }

    public void CheckIsContainerEmpty(Container container)
    {
        if (container.GetFilledSlotsCount() == 0)
        {
            this.StartCoroutine(this.SelfDestroyOnTime(4f));
        }
    }

    public void AttachMultipleContainers(List<Container> otherContainers)
    {
        Container newContainer = Container.MoveAllItemsInNewContainer(otherContainers);

        this.SetChestContainer(newContainer);
    }

    private IEnumerator SelfDestroyOnTime(float time)
    {
        yield return new WaitForSeconds(time);
        NetworkServer.Destroy(this.gameObject);
    }

    protected override void OnContainerSet()
    {
        base.OnContainerSet();
        this.Container.ContainerSlotsChanged += this.CheckIsContainerEmpty;
        this.CheckIsContainerEmpty(this.Container);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (isServer)
            this.Container.ContainerSlotsChanged -= this.CheckIsContainerEmpty;
    }
}
