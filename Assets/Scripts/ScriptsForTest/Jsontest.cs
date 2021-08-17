using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jsontest : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        string json = @"
        {
            ""armor"": 15,
            ""attack"": 12,
            ""wtf"": 13
        }";

        var node = JSONNode.Parse(json);
        if (node.Tag == JSONNodeType.Object)
        {
            foreach (KeyValuePair<string, JSONNode> kvp in (JSONObject)node)
            {
                //print(("{0}: {1}", kvp.Key, kvp.Value));

            }
        }
    }
}