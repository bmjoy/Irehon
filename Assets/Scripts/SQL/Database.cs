using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace MySql
{
    public static class Database
    {
        public static int Register(string login, int passsword)
        {
            try
            {
                Dictionary<string, string> userInfo = new Dictionary<string, string>();
                userInfo["p_id"] = null;
                userInfo["login"] = login;
                userInfo["password"] = passsword.ToString();
                int p_id = Convert.ToInt32(Connection.Insert("users", userInfo));
                return p_id;
            }
            catch
            {
                return -1;
            }
        }

        public static int Login(string login, int password)
        {
            var filter = new Dictionary<string, string>()
            {
                ["login"] = login,
                ["password"] = password.ToString()
            };
            Connection.SingleSelect("users", "p_id", filter);
            string response = Connection.SingleSelect("users", "p_id", filter);
            if (response != "")
                return Convert.ToInt32(response);
            else
                return 0;
        }

        public static bool CreateNewCharacter(int p_id, Character character)
        {
            try
            {
                Dictionary<string, string> characterInfo = new Dictionary<string, string>();
                characterInfo["c_id"] = null;
                characterInfo["nickname"] = character.NickName;
                characterInfo["p_id"] = p_id.ToString();
                characterInfo["container_id"] = "0";
                characterInfo["equipment_id"] = "0";
                string c_id_str = Connection.Insert("characters", characterInfo);
                if (c_id_str == null)
                    return false;
                int c_id = Convert.ToInt32(c_id_str);
                CreateAndLinkCharacterContainer(c_id);
                CreateCharacterData(c_id);
                CreatePositionData(c_id, character.position);

                //Test inventory system on new chars
                ContainerData.GiveCharacterItem(c_id, 1);
                ContainerData.GiveCharacterItem(c_id, 4);
                ContainerData.GiveCharacterItem(c_id, 3);
                ContainerData.GiveCharacterItem(c_id, 2);
                ContainerData.GiveCharacterItem(c_id, 2);

                return true;
            }
            catch 
            {
                return false;
            }
        }

        private static void CreateAndLinkCharacterContainer(int c_id)
        {
            int containerId = ContainerData.CreateContainer(20);
            int equipmentContainer = ContainerData.CreateContainer(Equipment.EquipmentSlotLength);

            Dictionary<string, string> updateValues = new Dictionary<string, string>();
            updateValues["container_id"] = containerId.ToString();
            updateValues["equipment_id"] = equipmentContainer.ToString();

            Connection.UpdateColumn("characters", "c_id", c_id.ToString(), updateValues);
        }

        private static void CreateCharacterData(int c_id)
        {
            Connection.Insert("c_data", "c_id", c_id.ToString());
        }

        public static List<Character> GetCharacters(int p_id)
        {
            int columnsInTable = 2;

            List<Character> characters = new List<Character>();

            string command = "SELECT nickname, c_id FROM characters WHERE p_id = " + p_id + " ORDER BY c_id;";

            List<string> characterInfo = Connection.RecieveMultipleData(command, columnsInTable);

            int charactersQuantity = characterInfo.Count / columnsInTable;

            for (int i = 0; i < charactersQuantity; i++)
            {
                characters.Add(
                new Character
                {
                    NickName = characterInfo[0 + i * columnsInTable],
                    slot = i,
                    id = Convert.ToInt32(characterInfo[1 + i * columnsInTable]),
                    position = Connection.GetVector3("SELECT p_x, p_y, p_z FROM c_positions " +
                        "WHERE c_id = " + characterInfo[1 + i * columnsInTable])
                });
            }
            return characters;
        }

        public static CharacterData GetCharacterData(int c_id)
        {
            CharacterData data = new CharacterData();
            
            data.containerId = ContainerData.GetCharacterContainer(c_id);
            data.inventory = ContainerData.GetContainer(data.containerId);

            data.equipmentContainerId = ContainerData.GetEquipmentContainer(c_id);
            data.equipment = ContainerData.GetContainer(data.equipmentContainerId);
            
            data.characterId = c_id;
            
            return data;
        }

        public static void UpdateCharacterData(int c_id, CharacterData data)
        {
            new Querry().UpdateColumn("containers", "id", data.containerId.ToString(), "slots", data.inventory.ToJson())
                .UpdateColumn("containers", "id", data.equipmentContainerId.ToString(), "slots", data.equipment.ToJson())
                .Run();
        }

        private static void CreatePositionData(int c_id, Vector3 pos)
        {
            string x = pos.x.ToString("0.00", new CultureInfo("en-US"));
            string y = pos.y.ToString("0.00", new CultureInfo("en-US"));
            string z = pos.z.ToString("0.00", new CultureInfo("en-US"));

            string command = $"INSERT INTO `c_positions` (`c_id`, `p_x`, `p_y`, `p_z`) " +
                $"VALUES ('{c_id}', '{x}', '{y}', '{z}')";
            Connection.RecieveSingleData(command);
        }

        public static void UpdatePositionData(int c_id, Vector3 pos)
        {
            string x = pos.x.ToString("0.00", new CultureInfo("en-US"));
            string y = pos.y.ToString("0.00", new CultureInfo("en-US"));
            string z = pos.z.ToString("0.00", new CultureInfo("en-US"));

            string command = $"UPDATE `c_positions` SET `p_x` = '{x}', `p_y` = '{y}', `p_z` = '{z}' " +
                $"WHERE `c_positions`.`c_id` = {c_id};";
            Connection.RecieveSingleData(command);
        }

        public static int GetCharacterId(string NickName)
        {
            string c_id = Connection.SingleSelect("characters", "c_id", "nickname", NickName);
            return c_id == "" ? 0 : Convert.ToInt32(c_id);
        }

        public static string GetItemsList()
        {
            string[] itemColumns = { "id", "slug", "stack", "type", "name", "description", "rarity", "modifiers", "metadata", "slot" };
            return Connection.RecieveJson("items", itemColumns);
        }
    }
}