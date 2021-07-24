using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace MySql
{
    class Database
    {
        public static Database instance;

        public Database(Connection connection)
        {
            if (instance == null)
                instance = this;

            this.connection = connection;
        }

        private Connection connection;

        public int Register(string login, int passsword)
        {
            try
            {
                Dictionary<string, string> userInfo = new Dictionary<string, string>();
                userInfo["p_id"] = null;
                userInfo["login"] = login;
                userInfo["password"] = passsword.ToString();
                int p_id = Convert.ToInt32(connection.Insert("users", userInfo));
                return p_id;
            }
            catch
            {
                return -1;
            }
        }

        public int Login(string login, int password)
        {
            var filter = new Dictionary<string, string>()
            {
                ["login"] = login,
                ["password"] = password.ToString()
            };
            connection.SingleSelect("users", "p_id", filter);
            string response = connection.SingleSelect("users", "p_id", filter);
            if (response != "")
                return Convert.ToInt32(response);
            else
                return 0;
        }

        public bool CreateNewCharacter(int p_id, Character character)
        {
            try
            {
                Dictionary<string, string> characterInfo = new Dictionary<string, string>();
                characterInfo["c_id"] = null;
                characterInfo["nickname"] = character.NickName;
                characterInfo["p_id"] = p_id.ToString();
                characterInfo["container_id"] = "0";
                string c_id_str = connection.Insert("characters", characterInfo);
                if (c_id_str == null)
                    return false;

                int c_id = Convert.ToInt32(c_id_str);
                CreateAndLinkCharacterContainer(c_id);
                CreateCharacterData(c_id);
                CreatePositionData(c_id, character.position);
                ContainerData.i.GiveCharacterItem(c_id, 1);
                ContainerData.i.GiveCharacterItem(c_id, 4);
                ContainerData.i.GiveCharacterItem(c_id, 3);
                ContainerData.i.GiveCharacterItem(c_id, 1);
                ContainerData.i.GiveCharacterItem(c_id, 2);
                return true;
            }
            catch 
            {
                return false;
            }
        }

        private void CreateAndLinkCharacterContainer(int c_id)
        {
            int container_id = ContainerData.i.CreateContainer(20);
            connection.RecieveSingleData($"UPDATE characters SET container_id = '{container_id}' WHERE c_id = {c_id};");
        }

        private void CreateCharacterData(int c_id)
        {
            connection.Insert("c_data", "c_id", c_id.ToString());
        }

        public List<Character> GetCharacters(int p_id)
        {
            int columnsInTable = 2;

            List<Character> characters = new List<Character>();

            string command = "SELECT nickname, c_id FROM characters WHERE p_id = " + p_id + " ORDER BY c_id;";

            List<string> characterInfo = connection.RecieveMultipleData(command, columnsInTable);

            int charactersQuantity = characterInfo.Count / columnsInTable;

            for (int i = 0; i < charactersQuantity; i++)
            {
                characters.Add(
                new Character
                {
                    NickName = characterInfo[0 + i * columnsInTable],
                    slot = i,
                    id = Convert.ToInt32(characterInfo[1 + i * columnsInTable]),
                    position = connection.GetVector3("SELECT p_x, p_y, p_z FROM c_positions " +
                        "WHERE c_id = " + characterInfo[1 + i * columnsInTable])
                });
            }
            return characters;
        }

        public CharacterData GetCharacterData(int c_id)
        {
            CharacterData data = new CharacterData();
            data.containerId = ContainerData.i.GetCharacterContainer(c_id);
            data.inventory = ContainerData.i.GetContainer(data.containerId);
            data.characterId = c_id;
            return data;
        }

        public void UpdateCharacterData(int c_id, CharacterData data)
        {
            connection.UpdateColumn("containers", "id", data.containerId.ToString(), "slots", data.inventory.ToJson());
        }

        private void CreatePositionData(int c_id, Vector3 pos)
        {
            string x = pos.x.ToString("0.00", new CultureInfo("en-US"));
            string y = pos.y.ToString("0.00", new CultureInfo("en-US"));
            string z = pos.z.ToString("0.00", new CultureInfo("en-US"));

            string command = $"INSERT INTO `c_positions` (`c_id`, `p_x`, `p_y`, `p_z`) " +
                $"VALUES ('{c_id}', '{x}', '{y}', '{z}')";
            connection.RecieveSingleData(command);
        }

        public void UpdatePositionData(int c_id, Vector3 pos)
        {
            string x = pos.x.ToString("0.00", new CultureInfo("en-US"));
            string y = pos.y.ToString("0.00", new CultureInfo("en-US"));
            string z = pos.z.ToString("0.00", new CultureInfo("en-US"));

            string command = $"UPDATE `c_positions` SET `p_x` = '{x}', `p_y` = '{y}', `p_z` = '{z}' " +
                $"WHERE `c_positions`.`c_id` = {c_id};";
            connection.RecieveSingleData(command);
        }

        public int GetCharacterId(string NickName)
        {
            var filter = new Dictionary<string, string>()
            {
                ["nickname"] = NickName
            };
            return Convert.ToInt32(connection.SingleSelect("characters", "c_id", filter));
        }

        public string GetItemsList()
        {
            string[] itemColumns = { "id", "slug", "type", "name", "description", "rarity", "modifiers", "metadata" };
            return connection.RecieveJson("items", itemColumns);
        }
    }
}