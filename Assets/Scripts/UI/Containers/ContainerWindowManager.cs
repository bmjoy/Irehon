using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ContainerType { Inventory, Equipment, Chest, None }

public class ContainerWindowManager : MonoBehaviour
{
    [SerializeField]
    private UIWindow chestWindow;

    [Header("Spawn slots transform")]
    [SerializeField]
    private RectTransform inventorySpawnSlotsTransform;
    [SerializeField]
    private RectTransform chestSpawnSlotsTransform;

    [SerializeField]
    private GameObject inventorySlotPrefab;

    [SerializeField]
    private RectTransform dragger;
    [SerializeField]
    private Image draggerImage;

    [SerializeField]
    private List<InventorySlotUI> equipmentSlots = new List<InventorySlotUI>();

    private List<InventorySlotUI> inventorySlots = new List<InventorySlotUI>();

    [SerializeField]
    private Canvas canvas;
    private PlayerContainerController playerContainerController;
    public static ContainerWindowManager i;

    private void Awake()
    {
        if (i != this && i != null)
        {
            Destroy(this);
        }
        else
        {
            i = this;
        }

        Player.OnPlayerIntializeEvent += this.PlayerIntialize;
    }

    private ContainerSlot GetContainerAttachedSlot(InventorySlotUI slot)
    {
        if (this.playerContainerController.Containers.ContainsKey(slot.type) && this.playerContainerController.Containers[slot.type] != null)
        {
            return this.playerContainerController.Containers[slot.type]?[slot.slotId];
        }
        else
        {
            return null;
        }
    }
    public void PlayerIntialize(Player player)
    {
        this.playerContainerController = player.GetComponent<PlayerContainerController>();
        this.playerContainerController.OnInventoryUpdate += this.UpdateInventory;
        this.playerContainerController.OnEquipmentUpdate += this.UpdateEquipment;
    }

    public void MoveSlots(InventorySlotUI from, InventorySlotUI to)
    {
        this.playerContainerController.MoveItem(from.type, from.slotId, to.type, to.slotId);
    }

    public void UseItemSlot(InventorySlotUI slot)
    {
        ContainerSlot containerSlot = this.GetContainerAttachedSlot(slot);

        Item item = ItemDatabase.GetItemById(containerSlot.itemId);

        if (item.type == ItemType.Weapon || item.type == ItemType.Armor)
        {
            if (slot.type != ContainerType.Equipment)
            {
                this.playerContainerController.MoveItem(slot.type, slot.slotId, ContainerType.Equipment, (int)item.equipmentSlot);
            }
            else
            {
                ContainerSlot emptySlot = this.playerContainerController.Containers[ContainerType.Inventory].GetEmptySlot();
                if (emptySlot != null)
                {
                    this.playerContainerController.MoveItem(slot.type, slot.slotId, ContainerType.Inventory, emptySlot.slotIndex);
                }
            }
        }
    }

    public void FastMoveSlot(InventorySlotUI from)
    {
        if (from.type == ContainerType.Chest)
        {
            ContainerSlot slot = this.playerContainerController.Containers[ContainerType.Inventory].GetEmptySlot();
            if (slot != null)
            {
                this.playerContainerController.MoveItem(from.type, from.slotId, ContainerType.Inventory, slot.slotIndex);
            }
        }

        if (from.type == ContainerType.Inventory)
        {
            if (this.playerContainerController.Containers.ContainsKey(ContainerType.Chest) && this.playerContainerController.Containers[ContainerType.Chest] != null)
            {
                ContainerSlot slot = this.playerContainerController.Containers[ContainerType.Chest].GetEmptySlot();
                if (slot != null)
                {
                    this.playerContainerController.MoveItem(from.type, from.slotId, ContainerType.Chest, slot.slotIndex);
                }
            }
        }
    }

    public void OpenChest(Container container)
    {
        this.FillChestWindow(container);
        this.chestWindow.Open();
    }

    public void CloseChest()
    {
        this.chestWindow.Close();
    }

    public void StopInterractOnServer()
    {
        this.playerContainerController?.GetComponent<PlayerInteracter>().StopInterractRpc();
    }

    public RectTransform GetDragger()
    {
        return this.dragger;
    }

    public Image GetDraggerImage()
    {
        return this.draggerImage;
    }

    public void UpdateEquipment(Container container)
    {
        for (int i = 0; i < this.equipmentSlots.Count; i++)
        {
            this.equipmentSlots[i].Intialize(container[i], this.canvas, ContainerType.Equipment);
        }
    }

    public void UpdateInventory(Container container)
    {
        if (container.slots.Length > this.inventorySlots.Count)
        {

            int diff = container.slots.Length - this.inventorySlots.Count;
            for (int i = 0; i < diff; i++)
            {
                GameObject slot = Instantiate(this.inventorySlotPrefab, this.inventorySpawnSlotsTransform);
                this.inventorySlots.Add(slot.GetComponent<InventorySlotUI>());
            }
        }
        for (int i = 0; i < container.slots.Length; i++)
        {
            this.inventorySlots[i].Intialize(container[i], this.canvas, ContainerType.Inventory);
        }
    }

    private void FillChestWindow(Container container)
    {
        foreach (Transform previousContainerSlot in this.chestSpawnSlotsTransform)
        {
            Destroy(previousContainerSlot.gameObject);
        }

        foreach (ContainerSlot containerSlot in container.slots)
        {
            GameObject slot = Instantiate(this.inventorySlotPrefab, this.chestSpawnSlotsTransform);
            slot.GetComponent<InventorySlotUI>().Intialize(containerSlot, this.canvas, ContainerType.Chest);
        }
    }
}
