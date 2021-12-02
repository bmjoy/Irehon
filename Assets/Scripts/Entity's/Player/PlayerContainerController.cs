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

    private Dictionary<ContainerType, int> containersId = new Dictionary<ContainerType, int>();

    public OnContainerUpdate OnInventoryUpdate { get; private set; } = new OnContainerUpdate();
    public OnContainerUpdate OnEquipmentUpdate { get; private set; } = new OnContainerUpdate();

    private CharacterInfo characterData => player.GetCharacterInfo();
    private int openedContainerId;

    private void Awake()
    {
        player = GetComponent<Player>();

        if (!player.isDataAlreadyRecieved)
            player.OnCharacterDataUpdateEvent.AddListener(Intialize);
        else
            Intialize(player.GetCharacterInfo());
    
        OnInventoryUpdate.AddListener(ContainerWindowManager.i.UpdateInventory);
        OnEquipmentUpdate.AddListener(ContainerWindowManager.i.UpdateEquipment);
    }

    private void Start()
    { 
    }

    public void Intialize(CharacterInfo info)
    {
        player.OnCharacterDataUpdateEvent.RemoveListener(Intialize);
        containersId[ContainerType.Equipment] = info.equipmentId;
        containersId[ContainerType.Inventory] = info.inventoryId;
    }

    private ContainerType GetContainerType(int containerId)
    {
        foreach (var keyValuePair in containersId)
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
    private void SendChestDataTargetRPC(int containerId, Container container)
    {
        containersId[ContainerType.Chest] = containerId;
        Containers[ContainerType.Chest] = container;
        ContainerWindowManager.i.OpenChest(container);
    }

    private int GetContainerId(ContainerType type)
    {
        switch (type)
        {
            case ContainerType.Inventory: return characterData.inventoryId;
            case ContainerType.Chest: return openedContainerId;
            case ContainerType.Equipment: return characterData.equipmentId;
            default: return 0;
        };
    }

    [TargetRpc]
    private void CloseChestTargetRpc()
    {
        containersId[ContainerType.Chest] = 0;
        Containers[ContainerType.Chest] = null;
        ContainerWindowManager.i.CloseChest();
    }

    [Server]
    public void CloseChest()
    {
        ContainerData.ContainerUpdateNotifier.UnSubscribe(openedContainerId, SendChestDataTargetRPC);
        chest?.OnDestroyEvent.RemoveListener(CloseChest);
        chest = null;
        openedContainerId = 0;

        CloseChestTargetRpc();
    }

    [Server]
    public void OpenChest(Chest chest, int containerId)
    {
        if (openedContainerId != 0 || chest == null || Vector3.Distance(chest.gameObject.transform.position, transform.position) > 8f)
            return;

        this.chest = chest;
        chest.OnDestroyEvent.AddListener(CloseChest);

        openedContainerId = containerId;

        Container chestContainer = ContainerData.LoadedContainers[openedContainerId];
        SendChestDataTargetRPC(openedContainerId, chestContainer);

        ContainerData.ContainerUpdateNotifier.Subscribe(openedContainerId, SendChestDataTargetRPC);
    }

    //from , to
    [Server]
    private void Equip(int equipmentSlot, int inventorySlot)
    {
        Container inventory = ContainerData.LoadedContainers[characterData.inventoryId];

        Item equipableItem = ItemDatabase.GetItemById(inventory[inventorySlot].itemId);

        if (equipableItem.type != ItemType.Armor && equipableItem.type != ItemType.Weapon)
            return;

        if ((EquipmentSlot)equipmentSlot != equipableItem.equipmentSlot)
            return;

        ContainerData.MoveSlotData(characterData.inventoryId, inventorySlot, characterData.equipmentId, equipmentSlot);
    }

    [Command]
    //from , to
    public void MoveItem(ContainerType firstType, int firstSlot, ContainerType secondType, int secondSlot)
    {
        int firstContainerId = GetContainerId(firstType);
        if (firstContainerId == 0)
            return;

        int secondContainerId = GetContainerId(secondType);
        if (secondContainerId == 0)
            return;

        if (firstType == ContainerType.Inventory && secondType == ContainerType.Equipment)
            Equip(secondSlot, firstSlot);
        else if (firstType == ContainerType.Equipment && secondType == ContainerType.Inventory)
            ContainerData.MoveSlotData(characterData.equipmentId, firstSlot, characterData.inventoryId, secondSlot);
        else if (firstContainerId == secondContainerId)
            ContainerData.SwapSlot(firstContainerId, firstSlot, secondSlot);
        else
            ContainerData.MoveSlotData(firstContainerId, firstSlot, secondContainerId, secondSlot);
    }
}
