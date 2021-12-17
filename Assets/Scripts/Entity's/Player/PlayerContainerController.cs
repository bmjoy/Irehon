using UnityEngine;
using System.Collections;
using Mirror;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class PlayerContainerController : NetworkBehaviour
{
    public Dictionary<ContainerType, Container> Containers = new Dictionary<ContainerType, Container>();
    public event Container.ContainerEventHandler OnInventoryUpdate;
    public event Container.ContainerEventHandler OnEquipmentUpdate;
   
    private Player player;
    private Chest chest;
    private CharacterInfo characterData => player.GetCharacterInfo();
    private Container openedContainer;

    public Container inventory;
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
            ServerContainersIntialize();
        }

        if (equipment != null && equipment.slots != null)
        {
            OnEquipmentUpdate?.Invoke(equipment);
        }
        
        if (Containers.ContainsKey(ContainerType.Inventory))
            OnInventoryUpdate?.Invoke(Containers[ContainerType.Inventory]);
        
        if (Containers.ContainsKey(ContainerType.Equipment))
            OnEquipmentUpdate?.Invoke(Containers[ContainerType.Equipment]);

    }

    [Server]
    private void ServerContainersIntialize()
    {
        OnInventoryUpdate += InventoryUpdateTargetRpc;
        OnEquipmentUpdate += EquipmentUpdateClientRpc;
        equipment = ContainerData.LoadedContainers[characterData.equipmentId];
        inventory = ContainerData.LoadedContainers[characterData.inventoryId];
        inventory.OnContainerUpdate += container => OnInventoryUpdate?.Invoke(container);
        equipment.OnContainerUpdate += container => OnEquipmentUpdate?.Invoke(container);
        OnInventoryUpdate?.Invoke(inventory);
        OnEquipmentUpdate?.Invoke(equipment);
    }

    [ClientRpc]
    public void EquipmentUpdateClientRpc(Container equipment)
    {
        print(equipment.ToJson().ToString());
        Containers[ContainerType.Equipment] = equipment;
        OnEquipmentUpdate?.Invoke(equipment);
    }

    [TargetRpc]
    public void InventoryUpdateTargetRpc(Container inventory)
    {
        print(inventory.ToJson().ToString());
        Containers[ContainerType.Inventory] = inventory;
        OnInventoryUpdate?.Invoke(inventory);
    }

    [TargetRpc]
    public void ChestUpdateTargetRpc(Container chest)
    {
        Containers[ContainerType.Chest] = chest;
        ContainerWindowManager.i.OpenChest(chest);
    }

    [Server]
    private Container GetContainer(ContainerType type)
    {
        switch (type)
        {
            case ContainerType.Inventory: return inventory;
            case ContainerType.Chest: return openedContainer;
            case ContainerType.Equipment: return equipment;
            default: return null;
        };
    }


    [TargetRpc]
    private void CloseChestTargetRpc()
    {
        ContainerWindowManager.i.CloseChest();
    }

    [Server]
    public void CloseChest(IInteractable interactable)
    {
        openedContainer.OnContainerUpdate -= ChestUpdateTargetRpc;
        chest.OnDestroyEvent -= CloseChest;
        chest = null;
        openedContainer = null;

        CloseChestTargetRpc();
    }

    [Server]
    public void OpenChest(Chest chest, Container container)
    {
        this.chest = chest;
        chest.OnDestroyEvent -= CloseChest;

        openedContainer = container;

        ChestUpdateTargetRpc(container);

        container.OnContainerUpdate += ChestUpdateTargetRpc;
    }

    //from , to
    [Server]
    private void Equip(int equipmentSlot, int inventorySlot)
    {
        Item equipableItem = inventory[inventorySlot].GetItem();

        if (equipableItem.type != ItemType.Armor && equipableItem.type != ItemType.Weapon)
            return;

        if ((EquipmentSlot)equipmentSlot != equipableItem.equipmentSlot)
            return;

        Container.MoveSlotData(inventory, inventorySlot, equipment, equipmentSlot);
    }

    private void EquipmentHook(Container old, Container newContainer)
    {
        if (newContainer != null && newContainer.slots != null)
        {
            OnEquipmentUpdate?.Invoke(newContainer);
        }
    }

    [Command]
    //from , to
    public void MoveItem(ContainerType firstType, int firstSlot, ContainerType secondType, int secondSlot)
    {
        Container firstContainer = GetContainer(firstType);
        if (firstContainer == null)
            return;

        Container secondContainer = GetContainer(secondType);
        if (secondContainer == null)
            return;

        if (firstType == ContainerType.Inventory && secondType == ContainerType.Equipment)
            Equip(secondSlot, firstSlot);
        else
            Container.MoveSlotData(firstContainer, firstSlot, secondContainer, secondSlot);
    }
}
