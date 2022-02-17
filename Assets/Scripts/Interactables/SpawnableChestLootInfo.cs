using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon.Interactable
{
    public class SpawnableChestLootInfo : SerializedMonoBehaviour
    {
        public class SpawnableLoot
        {
            public int itemId;
            public int minQuantity;
            public int maxQuantity;
            public float tickChance;
        }

        public List<SpawnableLoot> spawnableLoot;
    }
}