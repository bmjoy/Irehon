using Irehon;
using Irehon.Interactable;
using Mirror;
using System.Collections.Generic;

public class PlayerContainerController : NetworkBehaviour
{
    public Dictionary<ContainerType, Container> Containers = new Dictionary<ContainerType, Container>();
    public event Container.ContainerEventHandler OnInventoryUpdate;
    public event Container.ContainerEventHandler OnEquipmentUpdate;

    private Player player;
    private Chest chest;
    private CharacterInfo characterData => this.player.GetCharacterInfo();
    private Container openedContainer;

    public Container inventory;
    [SyncVar(hook = nameof(EquipmentHook))]
    public Container equipment;

    private void Awake()
    {
        this.player = this.GetComponent<Player>();
    }

    private void Start()
    {
        if (this.isServer)
        {
            this.ServerContainersIntialize();
        }

        if (this.equipment != null && this.equipment.slots != null)
        {
            OnEquipmentUpdate?.Invoke(this.equipment);
        }

        if (this.Containers.ContainsKey(ContainerType.Inventory))
        {
            OnInventoryUpdate?.Invoke(this.Containers[ContainerType.Inventory]);
        }

        if (this.Containers.ContainsKey(ContainerType.Equipment))
        {
            OnEquipmentUpdate?.Invoke(this.Containers[ContainerType.Equipment]);
        }
    }

    [Server]
    private void ServerContainersIntialize()
    {
        OnInventoryUpdate += this.InventoryUpdateTargetRpc;
        OnEquipmentUpdate += this.EquipmentUpdateClientRpc;
        this.equipment = ContainerData.LoadedContainers[this.characterData.equipmentId];
        this.inventory = ContainerData.LoadedContainers[this.characterData.inventoryId];
        this.inventory.OnContainerUpdate += container => OnInventoryUpdate?.Invoke(container);
        this.equipment.OnContainerUpdate += container => OnEquipmentUpdate?.Invoke(container);
        OnInventoryUpdate?.Invoke(this.inventory);
        OnEquipmentUpdate?.Invoke(this.equipment);
    }

    [ClientRpc]
    public void EquipmentUpdateClientRpc(Container equipment)
    {
        print(equipment.ToJson().ToString());
        this.Containers[ContainerType.Equipment] = equipment;
        OnEquipmentUpdate?.Invoke(equipment);
    }

    [TargetRpc]
    public void InventoryUpdateTargetRpc(Container inventory)
    {
        print(inventory.ToJson().ToString());
        this.Containers[ContainerType.Inventory] = inventory;
        OnInventoryUpdate?.Invoke(inventory);
    }

    [TargetRpc]
    public void ChestUpdateTargetRpc(Container chest)
    {
        this.Containers[ContainerType.Chest] = chest;
        ContainerWindowManager.i.OpenChest(chest);
    }

    [Server]
    private Container GetContainer(ContainerType type)
    {
        switch (type)
        {
            case ContainerType.Inventory: return this.inventory;
            case ContainerType.Chest: return this.openedContainer;
            case ContainerType.Equipment: return this.equipment;
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
        this.openedContainer.OnContainerUpdate -= this.ChestUpdateTargetRpc;
        this.chest.OnDestroyEvent -= this.CloseChest;
        this.chest = null;
        this.openedContainer = null;

        this.CloseChestTargetRpc();
    }

    [Server]
    public void OpenChest(Chest chest, Container container)
    {
        this.chest = chest;
        chest.OnDestroyEvent -= this.CloseChest;

        this.openedContainer = container;

        this.ChestUpdateTargetRpc(container);

        container.OnContainerUpdate += this.ChestUpdateTargetRpc;
    }

    //from , to
    [Server]
    private void Equip(int equipmentSlot, int inventorySlot)
    {
        Item equipableItem = this.inventory[inventorySlot].GetItem();

        if (equipableItem.type != ItemType.Armor && equipableItem.type != ItemType.Weapon)
        {
            return;
        }

        if ((EquipmentSlot)equipmentSlot != equipableItem.equipmentSlot)
        {
            return;
        }

        Container.MoveSlotData(this.inventory, inventorySlot, this.equipment, equipmentSlot);
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
        Container firstContainer = this.GetContainer(firstType);
        if (firstContainer == null)
        {
            return;
        }

        Container secondContainer = this.GetContainer(secondType);
        if (secondContainer == null)
        {
            return;
        }

        if (firstType == ContainerType.Inventory && secondType == ContainerType.Equipment)
        {
            this.Equip(secondSlot, firstSlot);
        }
        else
        {
            Container.MoveSlotData(firstContainer, firstSlot, secondContainer, secondSlot);
        }
    }
}
