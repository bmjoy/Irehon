using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum OpenedContainerType { Inventory, Equipment, OtherContainer}

public class InventoryManager : MonoBehaviour
{
    [SerializeField]
    private UIWindow inventoryWindow;
    [SerializeField]
    private UIWindow otherContainerWindow;
    [SerializeField]
    private RectTransform inventory;
    [SerializeField]
    private RectTransform containerWindow;

    [SerializeField]
    private GameObject inventorySlotPrefab;

    [SerializeField]
    private RectTransform dragger;
    [SerializeField]
    private Image draggerImage;

    private List<InventorySlotUI> equipmentSlots = new List<InventorySlotUI>();
    private List<InventorySlotUI> inventorySlots = new List<InventorySlotUI>();
    [SerializeField]
    private Canvas canvas; 
    private PlayerContainerController playerContainerController;
    public static InventoryManager instance;

    private void Awake()
    {
        if (instance != this && instance != null)
            Destroy(this);
        else
            instance = this;
    }

    public void PlayerIntialize(Player player)
    {
        playerContainerController = player.GetComponent<PlayerContainerController>();
        player.OnCharacterDataUpdateEvent.AddListener(x => UpdateInventory(x.inventory));
        player.OnCharacterDataUpdateEvent.AddListener(x => UpdateEquipment(x.equipment));
        if (player.isDataAlreadyRecieved)
            UpdateInventory(player.GetCharacterData().inventory);
    }

    public void MoveSlots(InventorySlotUI from, InventorySlotUI to)
    {
        playerContainerController.MoveItem(from.type, from.slotId, to.type, to.slotId);
    }

    public void OpenOtherContainer(Container container)
    {
        FillOtherContainer(containerWindow, container, OpenedContainerType.OtherContainer);
        otherContainerWindow.Open();
    }

    public void CloseOtherContainer()
    {
        otherContainerWindow.Close();
        CloseOtherContainerOnServer();
    }

    public void CloseOtherContainerOnServer() => playerContainerController.OtherContainerClosedRpc();

    public RectTransform GetDragger() => dragger;

    public Image GetDraggerImage() => draggerImage;

    private void UpdateEquipment(Container container)
    {
        if (equipmentSlots.Count != container.slots.Length)
            throw new System.Exception("Equipment slots count not equal to equipment type count");
        for (int i = 0; i < equipmentSlots.Count; i++)
        {
            equipmentSlots[i].Intialize(container[i], canvas, OpenedContainerType.Equipment);
        }
    }

    private void UpdateInventory(Container container)
    {
        if (container.slots.Length > inventorySlots.Count)
        {

            int diff = container.slots.Length - inventorySlots.Count;
            for (int i = 0; i < diff; i++)
            {
                GameObject slot = Instantiate(inventorySlotPrefab, inventory);
                inventorySlots.Add(slot.GetComponent<InventorySlotUI>());
            }
        }
        for (int i = 0; i < container.slots.Length; i++)
        {
            inventorySlots[i].Intialize(container[i], canvas, OpenedContainerType.Inventory);
        }
    }

    private void FillOtherContainer(RectTransform containerBG, Container container, OpenedContainerType type)
    {
        foreach (Transform previousContainerSlot in containerBG)
                Destroy(previousContainerSlot.gameObject);
        foreach (ContainerSlot containerSlot in container.slots)
        {
            GameObject slot = Instantiate(inventorySlotPrefab, containerBG);
            slot.GetComponent<InventorySlotUI>().Intialize(containerSlot, canvas, type);
        }
    }
}
