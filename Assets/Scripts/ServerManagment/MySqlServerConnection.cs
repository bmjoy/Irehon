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

    public void Init()
    {
        connection = new MySqlConnection("server = 134.209.21.121; user = server; database = players; password = FGPFHGOU@#HASDAKSD;");
        connection.Open();
        string sql = $"USE players;";
        MySqlCommand command = new MySqlCommand(sql, connection);
        var chars = command.ExecuteScalar();
        print("MySql inited");
        float a = 1.5f;
        print(a + " prosto");
        string abc = a.ToString("0.00", new CultureInfo("en-US"));
        print(abc + " to string");
    }

    public void Register(string email, string passsword)
    {
        string sql = $"INSERT INTO `users` (`id`, `email`, `password`, `characters`) VALUES (NULL, '{email}', '{passsword}', '{{}}');";
        MySqlCommand command = new MySqlCommand(sql, connection);
        var chars = command.ExecuteScalar();
        if (chars != null)
            print(chars.ToString());
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

    public List<Character> GetCharacters(int p_id)
    {
        int columnsInTable = 3;

        List<Character> characters = new List<Character>();

        List<string> characterInfo = RecieveMultipleData("SELECT nickname, class, c_id FROM characters WHERE p_id = " + p_id + " ORDER BY c_id;", columnsInTable);

        int charactersQuantity = characterInfo.Count / columnsInTable;

        for (int i = 0; i < charactersQuantity; i++)
        {
            characters.Add(
            new Character
            {
                NickName = characterInfo[0 + i * columnsInTable],
                Class = (Character.CharacterClass)Enum.Parse(typeof(Character.CharacterClass), characterInfo[1 + i * columnsInTable], true),
                slot = i,
                position = GetVector3("SELECT p_x, p_y, p_z FROM c_positions WHERE c_id = " + characterInfo[2 + i * columnsInTable])
            });
        }
        return characters;
    }

    public void UpdatePositionData(int c_id, Vector3 pos)
    {
        string x = pos.x.ToString("0.00", new CultureInfo("en-US"));
        string y = pos.y.ToString("0.00", new CultureInfo("en-US"));
        string z = pos.z.ToString("0.00", new CultureInfo("en-US"));
        string command = $"UPDATE `c_positions` SET `p_x` = '{x}', `p_y` = '{y}', `p_z` = '{z}' WHERE `c_positions`.`c_id` = {c_id};";
        print(command);
        string response = RecieveSingleData(command);
        
        print(response);
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

    public int Login(string email, string password)
    {
        string response = RecieveSingleData($"SELECT p_id FROM users WHERE email = '{email}' AND password = '{password}';");
        if (response != "")
            return Convert.ToInt32(response);
        else
            return 0;
    }
}
