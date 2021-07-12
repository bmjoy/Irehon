using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class JsonTest : MonoBehaviour
{


    private void Start()
    {
        //string jsonString = MySqlServerConnection.instance.GetItemsList();
        //JSONNode json = JSON.Parse(jsonString);
    }

    int i = 0;

    private void Update()
    {
        i++;
        if (i == 10)
        {
            string jsonString = MySqlServerConnection.instance.GetItemsList();
            JSONNode json = JSON.Parse(jsonString);
            print(json.ToString());
            print(json[0].ToString());
        }
    }
}
