using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;

public class JsonTest : MonoBehaviour
{
    public static string GetEmptyInventoryJson()
    {
        InventorySlot[] slots = new InventorySlot[20];
        for (int i = 0; i < slots.Length; i++)
            slots[i] = new InventorySlot(i);
        return JsonHelper.ToJson(slots);
    }

    [Serializable]
    public class Test<T>
    {
        public T[] slots;
    }

    private void Start()
    {
        print(GetEmptyInventoryJson());
    }

    
}
