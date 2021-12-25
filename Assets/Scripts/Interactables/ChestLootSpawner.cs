using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon.Interactable
{
    [RequireComponent(typeof(SpawnableChestLootInfo))]
    public class ChestLootSpawner : Chest
    {
        public int containerCapacity = 20;
        private List<SpawnableChestLootInfo.SpawnableLoot> spawnableLoot;

        private void Start()
        {
            SetChestContainer(new Container(containerCapacity));
            spawnableLoot = GetComponent<SpawnableChestLootInfo>().spawnableLoot;
            InvokeRepeating(nameof(OnSpawnLootTick), 5, 5);
        }

        private void OnSpawnLootTick()
        {
            float tickChance = Random.Range(0, 100f);
            foreach (var spawnLoot in spawnableLoot)
            {
                if (Container.FindItem(spawnLoot.itemId) != null)
                    continue;

                var emptySlot = Container.GetEmptySlot();
                if (emptySlot == null)
                    break;

                if (spawnLoot.tickChance >= tickChance)
                {
                    int itemCount = Random.Range(spawnLoot.minQuantity, spawnLoot.maxQuantity + 1);
                    Container.AddItem(spawnLoot.itemId, itemCount);
                }
            }
        }
    }
}