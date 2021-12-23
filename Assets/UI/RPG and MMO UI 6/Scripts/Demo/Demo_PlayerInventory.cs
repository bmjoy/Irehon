using UnityEngine;

namespace DuloGames.UI
{
    public class Demo_PlayerInventory : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Transform m_SlotsContainer;
#pragma warning restore 0649

        private void Start()
        {
            if (this.m_SlotsContainer != null)
            {
                // Grab all the slots in the container
                UIItemSlot[] slots = this.m_SlotsContainer.GetComponentsInChildren<UIItemSlot>();

                // Assign slots that have ID in the player prefs
                foreach (UIItemSlot slot in slots)
                {
                    int assignedID = PlayerPrefs.GetInt("InventorySlot" + slot.ID, 0);

                    // If we have an id, assign the slot by item info from the item database
                    if (assignedID > 0)
                    {
                        slot.Assign(UIItemDatabase.Instance.GetByID(assignedID));
                    }
                }
            }
        }

        private void OnEnable()
        {
            if (this.m_SlotsContainer != null)
            {
                // Grab all the slots in the container
                UIItemSlot[] slots = this.m_SlotsContainer.GetComponentsInChildren<UIItemSlot>();

                // Hook on assign and unassign events
                foreach (UIItemSlot slot in slots)
                {
                    slot.onAssign.AddListener(this.OnSlotAssigned);
                    slot.onUnassign.AddListener(this.OnSlotUnassigned);
                }
            }
        }

        private void OnDisable()
        {
            if (this.m_SlotsContainer != null)
            {
                // Grab all the slots in the container
                UIItemSlot[] slots = this.m_SlotsContainer.GetComponentsInChildren<UIItemSlot>();

                // Unhook on assign and unassign events
                foreach (UIItemSlot slot in slots)
                {
                    slot.onAssign.RemoveListener(this.OnSlotAssigned);
                    slot.onUnassign.RemoveListener(this.OnSlotUnassigned);
                }
            }
        }

        private void OnSlotAssigned(UIItemSlot slot)
        {
            PlayerPrefs.SetInt("InventorySlot" + slot.ID, slot.GetItemInfo().ID);
        }

        private void OnSlotUnassigned(UIItemSlot slot)
        {
            PlayerPrefs.SetInt("InventorySlot" + slot.ID, 0);
        }
    }
}