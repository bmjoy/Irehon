using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

public struct CharacterInfo
{
    public ulong id;
    public string name;
    public int inventory_id;
    public int equipment_id;
    public int serverId;
    public Vector3 position;

    public CharacterInfo(JSONNode json)
    {
        id = json["id"].AsULong;
        name = "";
        inventory_id = json["inventory_id"].AsInt;
        equipment_id = json["equipment_id"].AsInt;
        JSONNode pos = json["position"];
        serverId = json["server"].AsInt;
        position = new Vector3(pos["x"].AsFloat, pos["y"].AsFloat, pos["z"].AsFloat);
    }
}