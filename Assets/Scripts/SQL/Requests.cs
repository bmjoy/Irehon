using SimpleJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PersonalChestInfo
{
    public string ChestName;
    public int ContainerId;

    public PersonalChestInfo()
    {
         
    }

    public PersonalChestInfo(JSONObject json)
    {
        ChestName = json["name"];
        ContainerId = json["container_id"].AsInt;
    }

    public PersonalChestInfo(string name)
    {
        ChestName = name;
    }

    private JSONObject AsJson()
    {
        JSONObject json = new JSONObject();
        json["name"] = ChestName;
        json["container_id"] = ContainerId;

        return json;
    }

    public static List<PersonalChestInfo> GetChests(JSONNode json)
    {
        var personalChests = new List<PersonalChestInfo>();
        json = json["personal_chests"];

        if (json != null)
        {
            foreach (JSONObject chest in json)
                personalChests.Add(new PersonalChestInfo(chest));
        }
       
        return personalChests;
    }

    public static string ToJson(List<PersonalChestInfo> chests)
    {
        JSONArray jsonArray = new JSONArray();
        foreach (var chest in chests)
            jsonArray.Add(chest.AsJson());

        return jsonArray.ToString();
    }
}

public struct CharacterInfo
{
    public Fraction fraction;
    public bool isOnlineOnAnotherServer;
    public ulong steamId;
    public string name;
    public int inventoryId;
    public int equipmentId;
    public int serverId;
    public List<PersonalChestInfo> personalChests;
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
        steamId = json["id"].AsULong;
        name = "";
        inventoryId = json["inventory_id"].AsInt;
        equipmentId = json["equipment_id"].AsInt;
        JSONNode pos = json["position"];
        location = json["location"];
        serverId = json["server"].AsInt;

        personalChests = PersonalChestInfo.GetChests(json);

        position = new Vector3(pos["x"].AsFloat, pos["y"].AsFloat, pos["z"].AsFloat);
        spawnPoint = new Vector3(pos["spawn_x"].AsFloat, pos["spawn_y"].AsFloat, pos["spawn_z"].AsFloat);
        spawnSceneName = pos["spawn_location"];
        sceneChangeInfo = null;
        isSpawnPointChanged = false;
    }
}