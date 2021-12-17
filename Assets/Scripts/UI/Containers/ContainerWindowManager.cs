using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ContainerType { Inventory, Equipment, Chest, None}

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
            Destroy(this);
        else
            i = this;

        Player.OnPlayerIntializeEvent.AddListener(PlayerIntialize);
    }

    private ContainerSlot GetContainerAttachedSlot(InventorySlotUI slot)
    {
        if (playerContainerController.Containers.ContainsKey(slot.type) && playerContainerController.Containers[slot.type] != null)
            return playerContainerController.Containers[slot.type]?[slot.slotId];
        else
            return null;
    }
    public void PlayerIntialize(Player player)
    {
        playerContainerController = player.GetComponent<PlayerContainerController>();
        playerContainerController.OnInventoryUpdate += UpdateInventory;
        playerContainerController.OnEquipmentUpdate += UpdateEquipment;
    }

    public void MoveSlots(InventorySlotUI from, InventorySlotUI to)
    {
        playerContainerController.MoveItem(from.type, from.slotId, to.type, to.slotId);
    }

    public void UseItemSlot(InventorySlotUI slot)
    {
        var containerSlot = GetContainerAttachedSlot(slot);

        Item item = ItemDatabase.GetItemById(containerSlot.itemId);

        if (item.type == ItemType.Weapon || item.type == ItemType.Armor)
        {
            if (slot.type != ContainerType.Equipment)
                playerContainerController.MoveItem(slot.type, slot.slotId, ContainerType.Equipment, (int)item.equipmentSlot);
            else
            {
                var emptySlot = playerContainerController.Containers[ContainerType.Inventory].GetEmptySlot();
                if (emptySlot != null)
                    playerContainerController.MoveItem(slot.type, slot.slotId, ContainerType.Inventory, emptySlot.slotIndex);
            }
        }
    }

    public void FastMoveSlot(InventorySlotUI from)
    {
        if (from.type == ContainerType.Chest)
        {
            var slot = playerContainerController.Containers[ContainerType.Inventory].GetEmptySlot();
            if (slot != null)
                playerContainerController.MoveItem(from.type, from.slotId, ContainerType.Inventory, slot.slotIndex);
        }

        if (from.type == ContainerType.Inventory)
        {
            if (playerContainerController.Containers.ContainsKey(ContainerType.Chest) && playerContainerController.Containers[ContainerType.Chest] != null)
            {
                var slot = playerContainerController.Containers[ContainerType.Chest].GetEmptySlot();
                if (slot != null)
                    playerContainerController.MoveItem(from.type, from.slotId, ContainerType.Chest, slot.slotIndex);
            }
        }
    }

    public void OpenChest(Container container)
    {
        FillChestWindow(container);
        chestWindow.Open();
    }

    public void CloseChest() => chestWindow.Close();

    public void StopInterractOnServer() => playerContainerController?.GetComponent<PlayerInteracter>().StopInterractRpc();

    public RectTransform GetDragger() => dragger;

    public Image GetDraggerImage() => draggerImage;

    public void UpdateEquipment(Container container)
    {
        for (int i = 0; i < equipmentSlots.Count; i++)
        {
            equipmentSlots[i].Intialize(container[i], canvas, ContainerType.Equipment);
        }
    }

    public void UpdateInventory(Container container)
    {
        if (container.slots.Length > inventorySlots.Count)
        {

            int diff = container.slots.Length - inventorySlots.Count;
            for (int i = 0; i < diff; i++)
            {
                GameObject slot = Instantiate(inventorySlotPrefab, inventorySpawnSlotsTransform);
                inventorySlots.Add(slot.GetComponent<InventorySlotUI>());
            }
        }
        for (int i = 0; i < container.slots.Length; i++)
        {
            inventorySlots[i].Intialize(container[i], canvas, ContainerType.Inventory);
        }
    }

    private void FillChestWindow(Container container)
    {
        foreach (Transform previousContainerSlot in chestSpawnSlotsTransform)
                Destroy(previousContainerSlot.gameObject);
        foreach (ContainerSlot containerSlot in container.slots)
        {
            GameObject slot = Instantiate(inventorySlotPrefab, chestSpawnSlotsTransform);
            slot.GetComponent<InventorySlotUI>().Intialize(containerSlot, canvas, ContainerType.Chest);
        }
    }
}
