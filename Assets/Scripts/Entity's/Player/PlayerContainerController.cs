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
    public ContainerEvent OnInventoryUpdate { get; private set; } = new ContainerEvent();
    public ContainerEvent OnEquipmentUpdate { get; private set; } = new ContainerEvent();

    private Player player;
    private Chest chest;
    private CharacterInfo characterData => player.GetCharacterInfo();
    private int openedContainerId;

    [SyncVar(hook = nameof(EquipmentHook))]
    public Container equipment;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Start()
    {
        if (isServer)
        {
            OnInventoryUpdate.AddListener(InventoryUpdateTargetRpc);
            OnEquipmentUpdate.AddListener(EquipmentUpdateClientRpc);

            OnEquipmentUpdate.AddListener(equipment => this.equipment = equipment);
        }

        if (equipment != null && equipment.slots != null)
        {
            OnEquipmentUpdate.Invoke(equipment);
        }
        if (Containers.ContainsKey(ContainerType.Inventory))
            OnInventoryUpdate.Invoke(Containers[ContainerType.Inventory]);
        
        if (Containers.ContainsKey(ContainerType.Equipment))
            OnEquipmentUpdate.Invoke(Containers[ContainerType.Equipment]);
        if (isServer)
            ServerContainersReIntialize();
    }

    [Server]
    private void ServerContainersReIntialize()
    {
        Container equipment = ContainerData.LoadedContainers[characterData.equipmentId];
        Container inventory = ContainerData.LoadedContainers[characterData.inventoryId];
        OnInventoryUpdate.Invoke(inventory);
        OnEquipmentUpdate.Invoke(equipment);
    }

    [ClientRpc]
    public void EquipmentUpdateClientRpc(Container equipment)
    {
        Containers[ContainerType.Equipment] = equipment;
        OnEquipmentUpdate.Invoke(equipment);
    }

    [TargetRpc]
    public void InventoryUpdateTargetRpc(Container inventory)
    {
        Containers[ContainerType.Inventory] = inventory;
        OnInventoryUpdate.Invoke(inventory);
    }

    [TargetRpc]
    public void ChestUpdateTargetRpc(Container chest)
    {
        Containers[ContainerType.Chest] = chest;
        ContainerWindowManager.i.OpenChest(chest);
    }

    [Server]
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
        ContainerWindowManager.i.CloseChest();
    }

    [Server]
    public void CloseChest()
    {
        ContainerData.ContainerUpdateNotifier.UnSubscribe(openedContainerId, SendChestToPlayer);
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
        SendChestToPlayer(openedContainerId, chestContainer);

        ContainerData.ContainerUpdateNotifier.Subscribe(openedContainerId, SendChestToPlayer);
    }

    [Server]
    private void SendChestToPlayer(int containerId, Container container)
    {
        ChestUpdateTargetRpc(container);
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

    private void EquipmentHook(Container old, Container newContainer)
    {
        if (newContainer != null && newContainer.slots != null)
        {
            OnEquipmentUpdate.Invoke(newContainer);
        }
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
