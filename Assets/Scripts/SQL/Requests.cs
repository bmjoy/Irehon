using SimpleJSON;
using System;
using UnityEngine;

public struct CharacterInfo
{
    public Fraction fraction;
    public bool isOnlineOnAnotherServer;
    public ulong id;
    public string name;
    public int inventoryId;
    public int equipmentId;
    public int serverId;
    public string location;
    public Vector3 position;
    public SceneChangeInfo sceneChangeInfo;
    public Vector3 spawnPoint;
    public string spawnSceneName;
    public bool isSpawnPointChanged;

    public CharacterInfo(JSONNode json)
    {
        isOnlineOnAnotherServer = json["online"].AsInt != 0;
        fraction = (Fraction)Enum.Parse(typeof(Fraction), json["fraction"]);
        id = json["id"].AsULong;
        name = "";
        inventoryId = json["inventory_id"].AsInt;
        equipmentId = json["equipment_id"].AsInt;
        JSONNode pos = json["position"];
        location = json["location"];
        serverId = json["server"].AsInt;
        position = new Vector3(pos["x"].AsFloat, pos["y"].AsFloat, pos["z"].AsFloat);
        spawnPoint = new Vector3(pos["spawn_x"].AsFloat, pos["spawn_y"].AsFloat, pos["spawn_z"].AsFloat);
        spawnSceneName = pos["spawn_location"];
        sceneChangeInfo = null;
        isSpawnPointChanged = false;
    }
}