using UnityEngine;
using System.Collections;
using Mirror;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

public class PlayerContainerController : NetworkBehaviour
{
    [SerializeField]
    private GameObject deathPrefab;

    private Player player;
    private PlayerModelManager playerModelManager;
    private Chest chest;

    private CharacterData characterData => player.GetCharacterData();
    private int openedContainerId;

    private void Awake()
    {
        player = GetComponent<Player>();
        playerModelManager = GetComponent<PlayerModelManager>();
    }

    [Server]
    public void SpawnDeathContainer()
    {
        GameObject deathBody = Instantiate(deathPrefab);
        NetworkServer.Spawn(deathBody);
        deathBody.transform.position = player.GetMoldelPosition();
        deathBody.transform.rotation = transform.rotation;
        deathBody.GetComponent<RagdollController>().ChangeRagdollState(true);
        deathBody.GetComponent<DeathContainer>().SetEquipment(characterData.equipment);
        CharacterData newData = characterData;
        newData.equipment.Truncate();
        newData.inventory.Truncate();
        player.SetCharacterData(newData);
        int deathContainer = 0;
        var outer = Task.Factory.StartNew(() =>
        {
            List<int> playerContainers = new List<int>() 
            { 
                characterData.containerId, 
                characterData.equipmentContainerId 
            };
            deathContainer = MySql.ContainerData.MoveAllItemsInNewContainer(playerContainers);
        });
        StartCoroutine(WaitTask());
        IEnumerator WaitTask()
        {
            while (!outer.IsCompleted)
                yield return null;
            deathBody.GetComponent<Chest>().SetChestId(deathContainer);
        }
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
        chest?.OnContainerUpdate.RemoveListener(UpdateChestData);
        chest = null;
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
            case OpenedContainerType.Inventory: return characterData.containerId;
            case OpenedContainerType.OtherContainer: return openedContainerId;
            case OpenedContainerType.Equipment: return characterData.equipmentContainerId;
            default: return 0;
        };
    }

    [Server]
    public void OpenChest(Chest chest)
    {
        if (openedContainerId != 0 || chest == null ||
                Vector3.Distance(chest.gameObject.transform.position, transform.position) > 4f)
            return;

        this.chest = chest;

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
            chest = null;
            CloseChest();
        }
    }

    //from , to
    [Server]
    private void Equip(int equipmentSlot, int inventorySlot)
    {
        Item equipableItem = ItemDatabase.GetItemById(characterData.inventory[inventorySlot].itemId);

        if (equipableItem.type != ItemType.Armor && equipableItem.type != ItemType.Weapon)
            return;
        if ((EquipmentSlot)equipmentSlot != equipableItem.equipmentSlot)
            return;

        MySql.ContainerData.MoveSlot(characterData.containerId, inventorySlot, characterData.equipmentContainerId, equipmentSlot);
        {
            CharacterData characterData = this.characterData;
            characterData.inventory = MySql.ContainerData.GetContainer(characterData.containerId);
            characterData.equipment = MySql.ContainerData.GetContainer(characterData.equipmentContainerId);
            player.SetCharacterData(characterData);
        }
    }

    private bool IsMoveLegal(OpenedContainerType firstType, OpenedContainerType secondType)
    {
        if (firstType == OpenedContainerType.Equipment && secondType != OpenedContainerType.Inventory)
            return false;
        if (secondType == OpenedContainerType.Equipment && firstType != OpenedContainerType.Inventory)
            return false;
        else
            return true;
    }

    [Command]
    //from , to
    public void MoveItem(OpenedContainerType firstType, int firstSlot, OpenedContainerType secondType, int secondSlot)
    {
        if (!IsMoveLegal(firstType, secondType))
            return;
        int firstContainerId = GetContainerId(firstType);
        if (firstContainerId == 0)
            return;
        int secondContainerId = GetContainerId(secondType);
        if (secondContainerId == 0)
            return;

        Task.Run(() =>
        {
            if (firstType == OpenedContainerType.Inventory && secondType == OpenedContainerType.Equipment)
            {
                Equip(secondSlot, firstSlot);
                return;
            }
            if (firstType == OpenedContainerType.Equipment && secondType == OpenedContainerType.Inventory)
            {
                MySql.ContainerData.MoveSlot(characterData.equipmentContainerId, firstSlot, characterData.containerId, secondSlot);
                {
                    CharacterData characterData = this.characterData;
                    characterData.inventory = MySql.ContainerData.GetContainer(characterData.containerId);
                    characterData.equipment = MySql.ContainerData.GetContainer(characterData.equipmentContainerId);
                    player.SetCharacterData(characterData);
                }
            }
            else if (firstContainerId == secondContainerId)
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
                characterData.equipment = MySql.ContainerData.GetContainer(characterData.equipmentContainerId);
                player.SetCharacterData(characterData);
                if (firstType == OpenedContainerType.OtherContainer || secondType == OpenedContainerType.OtherContainer)
                    chest?.OnContainerUpdate.Invoke();
            }
        });
    }
}
