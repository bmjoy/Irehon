using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonTest : MonoBehaviour
{
    private void Start()
    {
        string json = Resources.Load<TextAsset>("Items").ToString();
        print(json);
    }
}
