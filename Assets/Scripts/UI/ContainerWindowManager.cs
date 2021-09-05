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
    }

    public void PlayerIntialize(Player player)
    {
        playerContainerController = player.GetComponent<PlayerContainerController>();
    }

    public void MoveSlots(InventorySlotUI from, InventorySlotUI to)
    {
        playerContainerController.MoveItem(from.type, from.slotId, to.type, to.slotId);
    }

    public void OpenChest(Container container)
    {
        FillChestWindow(container);
        chestWindow.Open();
    }

    public void CloseChest() => chestWindow.Close();

    public void CloseChestOnServer() => playerContainerController.ChestCloseUIRpc();

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
