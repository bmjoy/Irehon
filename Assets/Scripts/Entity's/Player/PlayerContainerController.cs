using UnityEngine;
using System.Collections;
using Mirror;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class PlayerContainerController : NetworkBehaviour
{
    public Dictionary<ContainerType, Container> Containers { get; private set; } = new Dictionary<ContainerType, Container>();

    private Player player;
    private Chest chest;

    private Dictionary<ContainerType, int> containers = new Dictionary<ContainerType, int>();

    public OnContainerUpdate OnInventoryUpdate { get; private set; } = new OnContainerUpdate();
    public OnContainerUpdate OnEquipmentUpdate { get; private set; } = new OnContainerUpdate();

    private CharacterInfo characterData => player.GetCharacterData();
    private int openedContainerId;

    private void Awake()
    {
        player = GetComponent<Player>();

        if (!player.isDataAlreadyRecieved)
            player.OnCharacterDataUpdateEvent.AddListener(Intialize);
        else
            Intialize(player.GetCharacterData());
        OnInventoryUpdate.AddListener(ContainerWindowManager.i.UpdateInventory);
        OnEquipmentUpdate.AddListener(ContainerWindowManager.i.UpdateEquipment);
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
        Containers[type] = container;
        switch (type)
        {
            case ContainerType.Inventory:
                OnInventoryUpdate.Invoke(container);
                break;
            case ContainerType.Equipment:
                OnEquipmentUpdate.Invoke(container);
                break;
            case ContainerType.Chest:
                ContainerWindowManager.i.OpenChest(container);
                break;
        }
    }

    [TargetRpc]
    private void SendChestData(int containerId, Container container)
    {
        containers[ContainerType.Chest] = containerId;
        Containers[ContainerType.Chest] = container;
        ContainerWindowManager.i.OpenChest(container);
    }

    [TargetRpc]
    public void SendItemDatabase(string json)
    {
        ItemDatabase.DatabaseLoadJson(json);
    }

    [Server]
    private void SendChestData(Container data)
    {
        StartCoroutine(Send());
        IEnumerator Send(){
            yield return ContainerData.LoadContainer(chest.ContainerId);
            Container container = ContainerData.LoadedContainers[chest.ContainerId];

            SendChestData(openedContainerId, container);
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

    [TargetRpc]
    private void CloseChestRpc()
    {
        containers[ContainerType.Chest] = 0;
        Containers[ContainerType.Chest] = null;
        ContainerWindowManager.i.CloseChest();
    }

    [Server]
    public void CloseChest()
    {
        chest?.OnContainerUpdate.RemoveListener(SendChestData);
        chest = null;
        openedContainerId = 0;

        CloseChestRpc();
    }

    [Server]
    public void OpenChest(Chest chest)
    {
        if (openedContainerId != 0 || chest == null || Vector3.Distance(chest.gameObject.transform.position, transform.position) > 8f)
            return;

        this.chest = chest;

        openedContainerId = chest.ContainerId;

        StartCoroutine(LoadAndSendChestData());
        IEnumerator LoadAndSendChestData()
        {
            yield return ContainerData.LoadContainer(chest.ContainerId);
            Container chestContainer = ContainerData.LoadedContainers[chest.ContainerId];
            SendChestData(chestContainer);
        }

        chest.OnContainerUpdate.AddListener(SendChestData);
    }

    //from , to
    [Server]
    private IEnumerator Equip(int equipmentSlot, int inventorySlot)
    {
        yield return ContainerData.LoadContainer(characterData.inventory_id);
        Container inventory = ContainerData.LoadedContainers[characterData.inventory_id];

        Item equipableItem = ItemDatabase.GetItemById(inventory[inventorySlot].itemId);

        if (equipableItem.type != ItemType.Armor && equipableItem.type != ItemType.Weapon)
            yield break;
        if ((EquipmentSlot)equipmentSlot != equipableItem.equipmentSlot)
            yield break;

        yield return ContainerData.MoveSlotData(characterData.inventory_id, inventorySlot, characterData.equipment_id, equipmentSlot);
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
            StartCoroutine(Equip(secondSlot, firstSlot));
        else if (firstType == ContainerType.Equipment && secondType == ContainerType.Inventory)
            StartCoroutine(ContainerData.MoveSlotData(characterData.equipment_id, firstSlot, characterData.inventory_id, secondSlot));
        else if (firstContainerId == secondContainerId)
            StartCoroutine(ContainerData.SwapSlot(firstContainerId, firstSlot, secondSlot));
        else
            StartCoroutine(ContainerData.MoveSlotData(firstContainerId, firstSlot, secondContainerId, secondSlot));
    }
}
