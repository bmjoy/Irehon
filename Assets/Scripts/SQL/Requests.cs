using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

public struct CharacterInfo
{
    public int id;
    public string name;
    public int inventory_id;
    public int equipment_id;
    public Container inventory;
    public Container equipment;
    public Vector3 position;

    public CharacterInfo(JSONNode json)
    {
        name = json["name"].Value;
        inventory_id = json["inventory_id"].AsInt;
        equipment_id = json["equipment_id"].AsInt;
        inventory = null;
        equipment = null;
        JSONNode pos = json["position"];
        position = new Vector3(pos["x"].AsFloat, pos["y"].AsFloat, pos["z"].AsFloat);
    }
}