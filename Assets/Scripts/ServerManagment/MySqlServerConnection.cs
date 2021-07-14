using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}

public class MySqlServerConnection : MonoBehaviour
{
    public static MySqlServerConnection instance;
    private MySqlConnection connection;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);
    }

    private void OnApplicationQuit()
    {
        connection.Close();
    }

    #region не открывать, тут для бд всё
    public void Init()
    {
        connection = new MySqlConnection("server = 134.209.21.121; " +
            "user = server; database = players; password = FGPFHGOU@#HASDAKSD;");
        connection.Open();
        RecieveSingleData("USE players;");
        ItemDatabase.instance.DatabaseLoad();
    }
    #endregion

    public int Register(string login, int passsword)
    {
        try
        {
            Dictionary<string, string> userInfo = new Dictionary<string, string>();
            userInfo["p_id"] = null;
            userInfo["login"] = login;
            userInfo["password"] = passsword.ToString();
            int p_id = Convert.ToInt32(InsertDictionary("users", userInfo));
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
        SingleSelect("users", "p_id", filter);
        string response = SingleSelect("users", "p_id", filter);
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
            string c_id_str = InsertDictionary("characters", characterInfo);
            if (c_id_str == null)
                return false;

            int c_id = Convert.ToInt32(c_id_str);
            Debug.Log("Creating container");
            CreateAndLinkCharacterContainer(c_id);
            Debug.Log("Creating char data");
            CreateCharacterData(c_id);
            Debug.Log("Creating position");
            CreatePositionData(c_id, character.position);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void CreateAndLinkCharacterContainer(int c_id)
    {
        int container_id = Convert.ToInt32(InsertSingleValue("containers", "slots", Inventory.GetEmptyInventoryJson()));
        RecieveSingleData($"UPDATE characters SET container_id = '{container_id}' WHERE c_id = {c_id};");
    }

    public void CreateCharacterData(int c_id)
    {
        InsertSingleValue("c_data", "c_id", c_id.ToString());
    }

    public List<Character> GetCharacters(int p_id)
    {
        int columnsInTable = 2;

        List<Character> characters = new List<Character>();

        string command = "SELECT nickname, c_id FROM characters WHERE p_id = " + p_id + " ORDER BY c_id;";

        List<string> characterInfo = RecieveMultipleData(command, columnsInTable);

        int charactersQuantity = characterInfo.Count / columnsInTable;

        for (int i = 0; i < charactersQuantity; i++)
        {
            characters.Add(
            new Character
            {
                NickName = characterInfo[0 + i * columnsInTable],
                slot = i,
                position = GetVector3("SELECT p_x, p_y, p_z FROM c_positions " +
                    "WHERE c_id = " + characterInfo[1 + i * columnsInTable])
            });
        }
        return characters;
    }

    public CharacterData GetCharacterData(int c_id)
    {
        CharacterData data = new CharacterData();
        //data.freeSkillPoints = Convert.ToInt32(RecieveSingleData($"SELECT c_freesp FROM c_data WHERE c_id = {c_id};"));
        //data.lvl = Convert.ToInt32(RecieveSingleData($"SELECT c_lvl FROM c_data WHERE c_id = {c_id};"));
        return data;
    }

    public void UpdateCharacterData(int c_id, CharacterData data)
    {
        //SendCommand($"UPDATE c_data SET c_freesp = '{data.freeSkillPoints}' WHERE c_id = {c_id};");
        //SendCommand($"UPDATE c_data SET c_lvl = '{data.lvl}' WHERE c_id = {c_id};");
    }

    public void CreatePositionData(int c_id, Vector3 pos)
    {
        string x = pos.x.ToString("0.00", new CultureInfo("en-US"));
        string y = pos.y.ToString("0.00", new CultureInfo("en-US"));
        string z = pos.z.ToString("0.00", new CultureInfo("en-US"));

        string command = $"INSERT INTO `c_positions` (`c_id`, `p_x`, `p_y`, `p_z`) " +
            $"VALUES ('{c_id}', '{x}', '{y}', '{z}')";
        string response = RecieveSingleData(command);
    }

    public void UpdatePositionData(int c_id, Vector3 pos)
    {
        string x = pos.x.ToString("0.00", new CultureInfo("en-US"));
        string y = pos.y.ToString("0.00", new CultureInfo("en-US"));
        string z = pos.z.ToString("0.00", new CultureInfo("en-US"));
        
        string command = $"UPDATE `c_positions` SET `p_x` = '{x}', `p_y` = '{y}', `p_z` = '{z}' " +
            $"WHERE `c_positions`.`c_id` = {c_id};";
        string response = RecieveSingleData(command);
    }

    public int GetCharacterId(string NickName)
    {
        var filter = new Dictionary<string, string>()
        {
            ["nickname"] = NickName
        };
        return Convert.ToInt32(SingleSelect("characters", "c_id", filter));
    }

    public string GetItemsList()
    {
        string[] itemColumns = { "id", "slug", "type", "name", "description", "rarity", "modifiers", "metadata" };
        return RecieveJson("items", itemColumns);
    }

    private string RecieveJson(string tableName, string[] columns)
    {
        string command = $"SELECT JSON_ARRAYAGG(JSON_OBJECT(";
        foreach (string column in columns)
        {
            if (command[command.Length - 1] != '(')
                command += ",";
            command += $"'{column}', {column}";
        }
        command += $")) FROM {tableName}";
        string recieve = RecieveSingleData(command);
        return recieve;
    }

    private List<string> RecieveMultipleData(string command, int columnQuantity)
    {
        List<string> response = new List<string>();

        MySqlCommand commandResponse = new MySqlCommand(command, connection);
        MySqlDataReader reader = commandResponse.ExecuteReader();
        while (reader.Read())
        {
            for (int i = 0; i < columnQuantity; i++)
                response.Add(reader[i].ToString());
        }
        reader.Close();

        return response;
    }

    private string SingleSelect(string table, string column, Dictionary<string, string> filter)
    {
        string command = $"SELECT {column} FROM {table}";
        if (filter.Count > 0)
        {
            command += " WHERE ";
            for (int i = 0; i < filter.Count; i++)
            {
                if (i != 0)
                    command += " AND ";
                var filterPair = filter.ElementAt(i);
                command += $"{filterPair.Key} = '{filterPair.Value}'";
            }
        }
        command += ";";
        return RecieveSingleData(command);
    }

    private string RecieveSingleData(string command)
    {
        MySqlCommand commandResponse = new MySqlCommand(command, connection);
        var chars = commandResponse.ExecuteScalar();
        if (chars != null)
            return chars.ToString();
        else
            return null;
    }

    private Vector3 GetVector3(string sqlCommand)
    {
        Vector3 vector = Vector3.zero;

        List<string> positionPoints = RecieveMultipleData(sqlCommand, 3);

        vector.x = (float)Convert.ToDouble(positionPoints[0]);
        vector.y = (float)Convert.ToDouble(positionPoints[1]);
        vector.z = (float)Convert.ToDouble(positionPoints[2]);

        return vector;
    }

    private string InsertSingleValue(string table, string key, string value)
    {
        return RecieveSingleData($"INSERT INTO {table} ({key}) VALUES ('{value}'); SELECT LAST_INSERT_ID();");
    }

    private string InsertDictionary(string table, Dictionary<string, string> values)
    {
        string[] keys = values.Keys.ToArray();
        string command = $"INSERT INTO `{table}` (";
        for (int i = 0; i < keys.Length; i++)
        {
            if (i != 0)
                command += ", ";
            command += $"`{keys[i]}`";
        }
        command += ") VALUES(";
        for (int i = 0; i < keys.Length; i++)
        {
            string value = values[keys[i]];
            if (i != 0)
                command += ", ";
            if (value != null)
                command += $"'{value}'";
            else
                command += "NULL";
        }
        command += "); SELECT LAST_INSERT_ID();";
        string res = RecieveSingleData(command);
        return res;
    }
}
