using UnityEngine;
using System.Collections;
using Mirror;
using System.Threading.Tasks;

public class PlayerContainerController : NetworkBehaviour
{
    private Player player;
    private CharacterData characterData => player.GetCharacterData();
    private int openedContainerId;
    private bool isContainerOpened;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    [TargetRpc]
    public void SendItemDatabase(string json)
    {
        ItemDatabase.DatabaseLoadJson(json);
    }

    [TargetRpc]
    public void OpenAnotherContainer(string json)
    {
        Container container = new Container(json);
        InventoryManager.instance.OpenOtherContainer(container);
    }

    [TargetRpc]
    private void CloseChest()
    {
        InventoryManager.instance.CloseOtherContainer();
    }

    [Command]
    public void OtherContainerClosedRpc()
    {
        openedContainerId = 0;
    }

    [Server]
    private void UpdateChestData()
    {
        OpenContainer(openedContainerId);
    }

    [Server]
    public void OpenContainer(int containerId)
    {
        openedContainerId = containerId;
        Task.Run(() =>
            OpenAnotherContainer(MySql.ContainerData.GetContainer(containerId).ToJson()));
    }

    private int GetContainerId(OpenedContainerType type)
    {
        switch (type)
        {
            case OpenedContainerType.Inventory: return player.GetCharacterData().containerId;
            case OpenedContainerType.OtherContainer: return openedContainerId;
            default: return 0;
        };
    }

    [Server]
    public void OpenChest(Chest chest)
    {
        if (openedContainerId != 0 || chest == null ||
                Vector3.Distance(chest.gameObject.transform.position, transform.position) > 4f)
            return;

        OpenContainer(chest.ContainerId);
        openedContainerId = chest.ContainerId;
        chest.OnContainerUpdate.AddListener(UpdateChestData);
        Vector3 chestPos = chest.transform.position;
        StartCoroutine(CheckChestDistance());
        IEnumerator CheckChestDistance()
        {
            while (openedContainerId != 0 && Vector3.Distance(chestPos, transform.position) < 4f)
            {
                yield return null;
            }
            openedContainerId = 0;
            chest.OnContainerUpdate.RemoveListener(UpdateChestData);
            CloseChest();
        }
    }

    [Command]
    public void MoveItem(OpenedContainerType firstType, int firstSlot, OpenedContainerType secondType, int secondSlot)
    {
        int firstContainerId = GetContainerId(firstType);
        if (firstContainerId == 0)
            return;
        int secondContainerId = GetContainerId(secondType);
        if (secondContainerId == 0)
            return;
        Task.Run(() =>
        {
            if (firstContainerId == secondContainerId)
            {
                MySql.ContainerData.SwapSlot(firstContainerId, firstSlot, secondSlot);
                if (firstContainerId == characterData.containerId)
                {
                    CharacterData characterData = this.characterData;
                    characterData.inventory = MySql.ContainerData.GetContainer(characterData.containerId);
                    player.SetCharacterData(characterData);
                }
                else
                    OpenContainer(firstContainerId);
            }
            else
            {
                MySql.ContainerData.MoveSlot(firstContainerId, firstSlot, secondContainerId, secondSlot);
                CharacterData characterData = this.characterData;
                characterData.inventory = MySql.ContainerData.GetContainer(characterData.containerId);
                player.SetCharacterData(characterData);
                if (firstContainerId != characterData.containerId)
                    OpenContainer(firstContainerId);
                else
                    OpenContainer(secondContainerId);
            }
        });
    }
}
