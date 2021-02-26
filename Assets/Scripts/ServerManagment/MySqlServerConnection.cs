using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

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
        SendCommand("USE players;");
    }
    #endregion

    public int Register(string email, string passsword)
    {
        string command = $"INSERT INTO `users` (`p_id`, `email`, `password`) " +
            $"VALUES (NULL, '{email}', '{passsword}')";
        RecieveSingleData(command);
        return (Login(email, passsword));
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
            string command = $"INSERT INTO `characters` (`c_id`, `nickname`, `class`, `p_id`) " +
                $"VALUES(NULL, '{character.NickName}', '{character.Class}', '{p_id}')";
            RecieveSingleData(command);
            int c_id = GetCharacterId(character.NickName);
            CreatePositionData(c_id, character.position);
            print("Created " + character.NickName);
            return true;
        }
        catch
        {
            print("Can't create character");
            return false;
        }
    }

    public List<Character> GetCharacters(int p_id)
    {
        int columnsInTable = 3;

        List<Character> characters = new List<Character>();

        string command = "SELECT nickname, class, c_id FROM characters WHERE p_id = " + p_id + " ORDER BY c_id;";

        List<string> characterInfo = RecieveMultipleData(command, columnsInTable);

        int charactersQuantity = characterInfo.Count / columnsInTable;

        for (int i = 0; i < charactersQuantity; i++)
        {
            characters.Add(
            new Character
            {
                NickName = characterInfo[0 + i * columnsInTable],
                Class = (Character.CharacterClass)Enum.Parse(typeof(Character.CharacterClass), characterInfo[1 + i * columnsInTable], true),
                slot = i,
                position = GetVector3("SELECT p_x, p_y, p_z FROM c_positions " +
                    "WHERE c_id = " + characterInfo[2 + i * columnsInTable])
            });
        }
        return characters;
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
        print(command);
        new MySqlCommand(command, connection);
    }

    private List<string> RecieveMultipleData(string command, int columnQuantity)
    {
        print(command);
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
        print(command);
        MySqlCommand commandResponse = new MySqlCommand(command, connection);
        var chars = commandResponse.ExecuteScalar();
        if (chars != null)
            return chars.ToString();
        else
            return null;
    }

    public int Login(string email, string password)
    {
        string response = RecieveSingleData($"SELECT p_id FROM users " +
            $"WHERE email = '{email}' AND password = '{password}';");
        if (response != "")
            return Convert.ToInt32(response);
        else
            return 0;
    }
}
