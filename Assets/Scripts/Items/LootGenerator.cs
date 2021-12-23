using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon.Entitys
{
    public class LootGenerator : SerializedMonoBehaviour
    {
        private struct Loot
        {
            public float chancePercent;
            public int itemId;
            public int minQuantity;
            public int maxQuantity;
        }

        [SerializeField]
        private List<Loot> lootPool = new List<Loot>();

        public Container GenerateLoot()
        {
            List<Loot> droppedLoot = new List<Loot>();

            foreach (Loot loot in this.lootPool)
            {
                float chance = Random.Range(0, 100);
                if (loot.chancePercent >= chance)
                {
                    droppedLoot.Add(loot);
                }
            }

            Container containerWithLoot = new Container(droppedLoot.Count);

            for (int i = 0; i < droppedLoot.Count; i++)
            {
                int itemQuantity = Random.Range(droppedLoot[i].minQuantity, droppedLoot[i].maxQuantity + 1);

                if (itemQuantity == 0)
                {
                    continue;
                }

                containerWithLoot[i].itemId = droppedLoot[i].itemId;
                containerWithLoot[i].itemQuantity = itemQuantity;
            }

            return containerWithLoot;
        }
    }
}