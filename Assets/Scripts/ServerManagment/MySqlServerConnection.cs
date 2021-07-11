using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
    }
    #endregion

    public int Register(string login, int passsword)
    {
        try
        {
            string command = $"INSERT INTO `users` (`p_id`, `login`, `password`) " +
                $"VALUES (NULL, '{login}', '{passsword}')";
            RecieveSingleData(command);
        }
        catch
        {
            return -1;
        }
        return (Login(login, passsword));
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

    public bool CreateNewCharacter(int p_id, Character character)
    {
        try
        {
            string command = $"INSERT INTO `characters` (`c_id`, `nickname`, `p_id`) " +
                $"VALUES(NULL, '{character.NickName}', '{p_id}')";
            RecieveSingleData(command);
            int c_id = GetCharacterId(character.NickName);
            if (c_id == 0)
                return false;
            CreateCharacterData(c_id);
            CreatePositionData(c_id, character.position);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void CreateCharacterData(int c_id)
    {
        string command = $"INSERT INTO c_data (c_id) VALUES ({c_id});";
        RecieveSingleData(command);
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
        return Convert.ToInt32(RecieveSingleData($"SELECT c_id FROM `characters` WHERE nickname = '{NickName}'"));
    }

    private void SendCommand(string command)
    {
        new MySqlCommand(command, connection);
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

    private string RecieveSingleData(string command)
    {
        MySqlCommand commandResponse = new MySqlCommand(command, connection);
        var chars = commandResponse.ExecuteScalar();
        if (chars != null)
            return chars.ToString();
        else
            return null;
    }

    public int Login(string login, int password)
    {
        string response = RecieveSingleData($"SELECT p_id FROM users " +
            $"WHERE login = '{login}' AND password = '{password}';");
        if (response != "")
            return Convert.ToInt32(response);
        else
            return 0;
    }
}
