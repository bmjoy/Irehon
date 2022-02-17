using SimpleJSON;
using System.Collections.Generic;

namespace Irehon
{
    public class PersonalChestInfo
    {
        public string ChestName;
        public int ContainerId;

        public PersonalChestInfo()
        {

        }

        public PersonalChestInfo(JSONObject json)
        {
            this.ChestName = json["name"];
            this.ContainerId = json["container_id"].AsInt;
        }

        public PersonalChestInfo(string name)
        {
            this.ChestName = name;
        }

        private JSONObject AsJson()
        {
            JSONObject json = new JSONObject();
            json["name"] = this.ChestName;
            json["container_id"] = this.ContainerId;

            return json;
        }

        public static List<PersonalChestInfo> GetChests(JSONNode json)
        {
            List<PersonalChestInfo> personalChests = new List<PersonalChestInfo>();
            json = json["personal_chests"];

            if (json != null)
            {
                foreach (JSONObject chest in json)
                {
                    personalChests.Add(new PersonalChestInfo(chest));
                }
            }

            return personalChests;
        }

        public static string ToJson(List<PersonalChestInfo> chests)
        {
            JSONArray jsonArray = new JSONArray();
            foreach (PersonalChestInfo chest in chests)
            {
                jsonArray.Add(chest.AsJson());
            }

            return jsonArray.ToString();
        }
    }
}