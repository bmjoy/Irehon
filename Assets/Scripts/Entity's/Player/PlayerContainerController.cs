using UnityEngine;
using System.Collections;
using Mirror;
using System.Threading.Tasks;
using System;

public class PlayerContainerController : NetworkBehaviour
{
    private Player player;
    private PlayerModelManager playerModelManager;
    private CharacterData characterData => player.GetCharacterData();
    private int openedContainerId;

    private void Awake()
    {
        player = GetComponent<Player>();
        playerModelManager = GetComponent<PlayerModelManager>();
    }

    [ClientRpc]
    public void UpdateEquipmentModelRpc(Container equipment)
    {
        for (int i = 0; i < equipment.slots.Length; i++)
        {
            if (equipment.slots[i].itemId != 0)
            {
                Item item = ItemDatabase.GetItemById(equipment[i].itemId);
                playerModelManager.EquipModel(item, item.equipmentSlot);
            }
            else
                playerModelManager.EquipModel(null, (EquipmentSlot)i);
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

    //from , to
    [Server]
    private void Equip(int equipmentSlot, int inventorySlot)
    {
        print("got equip");
        Item equipableItem = ItemDatabase.GetItemById(characterData.inventory[inventorySlot].itemId);

        if (equipableItem.type != ItemType.Armor || equipableItem.type != ItemType.Weapon)
            return;

        int equipmentItemSlot = equipableItem.metadata["equipmentSlot"].AsInt;
        
        if (equipmentSlot != equipmentItemSlot)
            return;

        MySql.ContainerData.MoveSlot(characterData.containerId, inventorySlot, characterData.equipmentContainerId, equipmentSlot);
        {
            CharacterData characterData = this.characterData;
            characterData.inventory = MySql.ContainerData.GetContainer(characterData.containerId);
            characterData.equipment = MySql.ContainerData.GetContainer(characterData.equipmentContainerId);
            player.SetCharacterData(characterData);
        }
        UpdateEquipmentModelRpc(characterData.equipment);
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
        print("got move item");
        if (!IsMoveLegal(firstType, secondType))
            return;
        int firstContainerId = GetContainerId(firstType);
        if (firstContainerId == 0)
            return;
        int secondContainerId = GetContainerId(secondType);
        if (secondContainerId == 0)
            return;
        print("got move item and start");

        Task.Run(() =>
        {
            if (firstType == OpenedContainerType.Inventory && secondType == OpenedContainerType.Equipment)
            {
                Equip(secondSlot, firstSlot);
                return;
            }
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
