using System.Collections.Generic;
using UnityEngine;

namespace Irehon.UI
{
    public class InventoryUI : MonoBehaviour
    {
        public static InventoryUI Instance { get; private set; }

        [Header("Spawn slots transform")]
        [SerializeField]
        private RectTransform inventorySpawnSlotsTransform;
        [SerializeField]
        private GameObject inventorySlotPrefab;

        private List<InventorySlotUI> inventorySlots = new List<InventorySlotUI>();
        private Canvas canvas;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            this.canvas = this.GetComponentInParent<Canvas>();
            if (this.canvas == null)
            {
                Debug.LogError("Intialize error, inventory should have canvas or be it's children");
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
                this.inventorySlots[i].Intialize(container[i], ContainerType.Inventory);
            }
        }
    }
}