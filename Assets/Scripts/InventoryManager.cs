using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField]
    private GameObject inventorySlotPrefab;
    [SerializeField]
    private RectTransform inventory;
    [SerializeField]
    private RectTransform containerWindow;
    public static InventoryManager instance;

    private void Awake()
    {
        if (instance != this && instance != null)
            Destroy(this);
        else
            instance = this;
    }

    public void OpenInventory(Container container)
    {
        FillContainer(inventory, container);
    }

    private void FillContainer(RectTransform containerBG, Container container)
    {
        foreach (ContainerSlot containerSlot in container.slots)
        {

        }
    }
}
