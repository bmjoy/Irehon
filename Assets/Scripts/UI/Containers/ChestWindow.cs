using System.Collections;
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

        public void CloseChest()
        {
            chestWindow.Close();
        }

        public void StopInterractOnServer()
        {
            player.GetComponent<PlayerInteracter>().StopInterractRpc();
        }
    }
}