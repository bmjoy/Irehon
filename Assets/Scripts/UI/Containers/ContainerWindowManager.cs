using Irehon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ContainerType { Inventory, Equipment, Chest, None }

public class ContainerWindowManager : MonoBehaviour
{

    
    [SerializeField]
    private GameObject inventorySlotPrefab;

    [SerializeField]
    private RectTransform dragger;
    [SerializeField]
    private Image draggerImage;

    [SerializeField]
    private Canvas canvas;
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

        Player.LocalPlayerIntialized += this.PlayerIntialize;
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
        Player.LocalInventorUpdated += UpdateInventory;
        Player.LocalEquipmentUpdated += UpdateEquipment;
    }

    public void MoveSlots(InventorySlotUI from, InventorySlotUI to)
    {
        this.playerContainerController.MoveItem(from.type, from.slotId, to.type, to.slotId);
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

    public RectTransform GetDragger()
    {
        return this.dragger;
    }

    public Image GetDraggerImage()
    {
        return this.draggerImage;
    }
}
