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

    private Dictionary<ContainerType, int> containers;

    private CharacterInfo characterData => player.GetCharacterData();
    private int openedContainerId;

    private void Awake()
    {
        player = GetComponent<Player>();
        playerModelManager = GetComponent<PlayerModelManager>();

        if (isLocalPlayer)
        {
            if (!player.isDataAlreadyRecieved)
                player.OnCharacterDataUpdateEvent.AddListener(Intialize);
            else
                Intialize(player.GetCharacterData());
        }
    }

    public void Intialize(CharacterInfo info)
    {
        player.OnCharacterDataUpdateEvent.RemoveListener(Intialize);
        containers[ContainerType.Equipment] = info.equipment_id;
        containers[ContainerType.Inventory] = info.inventory_id;
    }

    private ContainerType GetContainerType(int containerId)
    {
        foreach (var keyValuePair in containers)
        {
            if (keyValuePair.Value == containerId)
                return keyValuePair.Key;
        }
        return ContainerType.None;
    }

    [TargetRpc]
    public void SendContainerData(int containerId, Container container)
    {
        ContainerType type = GetContainerType(containerId);

        switch (type)
        {
            case ContainerType.Inventory:
                break;
            case ContainerType.Equipment:
                break;
            case ContainerType.Chest:
                break;
        }
    }

    [TargetRpc]
    public void SendItemDatabase(string json)
    {
        Debug.Log(json);
        ItemDatabase.DatabaseLoadJson(json);
    }

    [Command]
    public void OtherContainerClosedRpc()
    {
        chest?.OnContainerUpdate.RemoveListener(SendChestData);
        chest = null;
        openedContainerId = 0;
    }

    [Server]
    private void SendChestData()
    {
        StartCoroutine(Send());
        IEnumerator Send(){
            yield return ContainerData.LoadContainer(chest.ContainerId);
            Container container = ContainerData.LoadedContainers[chest.ContainerId];

            SendContainerData(openedContainerId, container);
        }
    }

    private int GetContainerId(ContainerType type)
    {
        switch (type)
        {
            case ContainerType.Inventory: return characterData.inventory_id;
            case ContainerType.Chest: return openedContainerId;
            case ContainerType.Equipment: return characterData.equipment_id;
            default: return 0;
        };
    }

    [Server]
    public void OpenChest(Chest chest)
    {

        if (openedContainerId != 0 || chest == null || Vector3.Distance(chest.gameObject.transform.position, transform.position) > 4f)
            return;

        this.chest = chest;

        openedContainerId = chest.ContainerId;

        SendChestData();
             
        chest.OnContainerUpdate.AddListener(SendChestData);

        Vector3 chestPos = chest.transform.position;

        StartCoroutine(CheckChestDistance());

        IEnumerator CheckChestDistance()
        {
            while (openedContainerId != 0 && Vector3.Distance(chestPos, transform.position) < 4f)
            {
                yield return null;
            }
            openedContainerId = 0;
            chest.OnContainerUpdate.RemoveListener(SendChestData);
            chest = null;
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

        StartCoroutine(ContainerData.MoveSlotData(characterData.inventory_id, inventorySlot, characterData.equipment_id, equipmentSlot));
    }

    private bool IsMoveLegal(ContainerType firstType, ContainerType secondType)
    {
        if (firstType == ContainerType.Equipment && secondType != ContainerType.Inventory)
            return false;
        if (secondType == ContainerType.Equipment && firstType != ContainerType.Inventory)
            return false;
        else
            return true;
    }

    [Command]
    //from , to
    public void MoveItem(ContainerType firstType, int firstSlot, ContainerType secondType, int secondSlot)
    {
        if (!IsMoveLegal(firstType, secondType))
            return;

        int firstContainerId = GetContainerId(firstType);
        if (firstContainerId == 0)
            return;

        int secondContainerId = GetContainerId(secondType);
        if (secondContainerId == 0)
            return;

        if (firstType == ContainerType.Inventory && secondType == ContainerType.Equipment)
        {
            Equip(secondSlot, firstSlot);
            return;
        }

        if (firstType == ContainerType.Equipment && secondType == ContainerType.Inventory)
            StartCoroutine(ContainerData.MoveSlotData(characterData.equipment_id, firstSlot, characterData.inventory_id, secondSlot));
        else if (firstContainerId == secondContainerId)
            StartCoroutine(ContainerData.SwapSlot(firstContainerId, firstSlot, secondSlot));
        else
            StartCoroutine(ContainerData.MoveSlotData(firstContainerId, firstSlot, secondContainerId, secondSlot));
    }
}
