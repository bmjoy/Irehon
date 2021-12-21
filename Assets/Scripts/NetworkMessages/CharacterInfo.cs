using Irehon.Entitys;
using SimpleJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon
{
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
            this.isOnlineOnAnotherServer = json["online"].AsInt != 0;
            this.fraction = (Fraction)Enum.Parse(typeof(Fraction), json["fraction"]);
            this.steamId = json["id"].AsULong;
            this.name = "";
            this.inventoryId = json["inventory_id"].AsInt;
            this.equipmentId = json["equipment_id"].AsInt;
            JSONNode pos = json["position"];
            this.location = json["location"];
            this.serverId = json["server"].AsInt;

            this.personalChests = PersonalChestInfo.GetChests(json);

            this.position = new Vector3(pos["x"].AsFloat, pos["y"].AsFloat, pos["z"].AsFloat);
            this.spawnPoint = new Vector3(pos["spawn_x"].AsFloat, pos["spawn_y"].AsFloat, pos["spawn_z"].AsFloat);
            this.spawnSceneName = pos["spawn_location"];
            this.sceneChangeInfo = null;
            this.isSpawnPointChanged = false;
        }
    }
}