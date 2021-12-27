using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon.UI
{
    public class EquipmentWindow : MonoBehaviour
    {
        [SerializeField]
        private List<InventorySlotUI> equipmentSlots = new List<InventorySlotUI>();

        private Canvas canvas;

        private void Awake()
        {
            PlayerContainers.LocalEquipmentUpdated += UpdateEquipmentWindow;
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                Debug.LogError("Equipment window not intialized, missing canvas");
        }

        public void UpdateEquipmentWindow(Container container)
        {
            for (int i = 0; i < this.equipmentSlots.Count; i++)
            {
                this.equipmentSlots[i].Intialize(container[i], ContainerType.Equipment);
            }
        } 

        private void OnDestroy()
        {
            PlayerContainers.LocalEquipmentUpdated -= UpdateEquipmentWindow;
        }
    }
}