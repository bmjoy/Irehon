using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon.UI
{
    public class ChestWindow : MonoBehaviour
    {
        public static ChestWindow Instance { get; private set; }
        [SerializeField]
        private UIWindow chestWindow;
        [SerializeField]
        private RectTransform chestSpawnSlotsTransform;
        [SerializeField]
        private GameObject inventorySlotPrefab;

        private List<InventorySlotUI> spawnedSlots = new List<InventorySlotUI>();
        private Canvas canvas;
        private Player player;

        private void Awake()
        {
            Instance = this;

            canvas = GetComponentInParent<Canvas>();
            Player.LocalPlayerIntialized += player => this.player = player;
            if (canvas == null)
                Debug.LogError("Chest window not intialized, missing canvas");
        }

        public void OpenChest(Container container)
        {
            this.RecreateInventorySlots(container);
            this.chestWindow.Open();
        }

        private void RecreateInventorySlots(Container container)
        {
            if (container.slots.Length > spawnedSlots.Count)
            {
                int diff = container.slots.Length - spawnedSlots.Count;
                for (int i = 0; i < diff; i++)
                {
                    GameObject slot = Instantiate(this.inventorySlotPrefab, chestSpawnSlotsTransform);
                    spawnedSlots.Add(slot.GetComponent<InventorySlotUI>());
                }
            }
            else if (container.slots.Length < spawnedSlots.Count)
            {
                int diff = spawnedSlots.Count - container.slots.Length;
                for (int i = 0; i < diff; i++)
                {
                    GameObject slot = spawnedSlots[0].gameObject;
                    spawnedSlots.RemoveAt(0);
                    Destroy(slot);
                }
            }

            for (int i = 0; i < container.slots.Length; i++)
            {
                var slot = spawnedSlots[i];
                slot.Intialize(container[i], ContainerType.Interact);
            }
        }

        public void CloseChest()
        {
            chestWindow.Close();
        }

        public void StopInterractOnServer()
        {
            player.GetComponent<PlayerInteracter>().StopInterractCommand();
        }
    }
}