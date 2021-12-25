using Irehon.Interactable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon.UI
{
    public class LootWindow : MonoBehaviour
    {
        public static LootWindow Instance { get; private set; }
        [SerializeField]
        private GameObject lootTabPrefab;
        [SerializeField]
        private Transform lootTabsSpawnTransform;
        [SerializeField]
        private UIWindow lootbagWindow;

        private List<LootTab> spawnedLootTabs = new List<LootTab>();

        private void Awake()
        {
            Instance = this;
        }

        public void OpenLootBag(Container container)
        {
            if (container.GetFilledSlotsCount() > spawnedLootTabs.Count)
            {
                int diff = container.GetFilledSlotsCount() - spawnedLootTabs.Count;
                for (int i = 0; i < diff; i++)
                {
                    GameObject slot = Instantiate(lootTabPrefab, lootTabsSpawnTransform);
                    spawnedLootTabs.Add(slot.GetComponent<LootTab>());
                }
            }
            else if (container.GetFilledSlotsCount() < spawnedLootTabs.Count)
            {
                int diff = spawnedLootTabs.Count - container.GetFilledSlotsCount();
                for (int i = 0; i < diff; i++)
                {
                    GameObject slot = spawnedLootTabs[0].gameObject;
                    spawnedLootTabs.RemoveAt(0);
                    Destroy(slot);
                }
            }

            var filledSlots = container.GetFilledSlots();
            for (int i = 0; i < container.GetFilledSlotsCount(); i++)
            {
                var slot = spawnedLootTabs[i];
                slot.Intialize(filledSlots[i]);
            }

            lootbagWindow.Open();
        }

        public void CloseLootBag()
        {
            lootbagWindow.Close();
        }

        public void ClaimItem(int index, int itemCount)
        {
            if (PlayerInteracter.LocalInteractObject?.GetComponent<LootBag>() != null)
            {
                PlayerInteracter.LocalInteractObject.GetComponent<LootBag>().ClaimItem(index, itemCount);
            }
        }
    }
}