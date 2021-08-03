using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum OpenedContainerType { Inventory, OtherContainer}

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

    private List<InventorySlotUI> inventorySlots = new List<InventorySlotUI>();
    private Canvas canvas;
    private Player player;
    public static InventoryManager instance;

    private void Awake()
    {
        if (instance != this && instance != null)
            Destroy(this);
        else
            instance = this;
        canvas = GetComponent<Canvas>();
    }

    public void PlayerIntialize(Player player)
    {
        this.player = player;
        player.OnCharacterDataUpdateEvent.AddListener(x => FillInventory(x.inventory));
        if (player.isDataAlreadyRecieved)
            FillInventory(player.GetCharacterData().inventory);
    }

    public void MoveSlots(InventorySlotUI from, InventorySlotUI to)
    {
        player.MoveItem(from.type, from.slotId, to.type, to.slotId);
    }

    public void OpenContainer(Container container)
    {
        FillContainer(containerWindow, container, OpenedContainerType.OtherContainer);
        otherContainerWindow.Open();
    }

    public void CloseContainer()
    {
        otherContainerWindow.Close();
    }

    public RectTransform GetDragger() => dragger;

    public Image GetDraggerImage() => draggerImage;

    private void FillInventory(Container container)
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

    private void FillContainer(RectTransform containerBG, Container container, OpenedContainerType type)
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
