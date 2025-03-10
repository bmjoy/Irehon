﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon.UI
{
    public class InventoryWindow : MonoBehaviour
    {
        [SerializeField]
        private RectTransform inventorySpawnSlotsTransform;
        [SerializeField]
        private GameObject inventorySlotPrefab;

        private List<InventorySlotUI> inventorySlots = new List<InventorySlotUI>();

        private void Awake()
        {
            PlayerContainers.LocalInventorUpdated += ReCreateInventorySlotsUI;
        }
        public void ReCreateInventorySlotsUI(Container container)
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
                this.inventorySlots[i].Intialize(container[i], ContainerType.Inventory);
            }
        }

        private void OnDestroy()
        {
            PlayerContainers.LocalInventorUpdated -= ReCreateInventorySlotsUI;
        }
    }
}