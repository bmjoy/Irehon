using UnityEngine;
using MySql.Data.MySqlClient;
using System.Collections;

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

    public void Init()
    {
        connection = new MySqlConnection("server = 134.209.21.121; user = username; database = players; password = password;");
        connection.Open();
        string sql = $"USE players;";
        MySqlCommand command = new MySqlCommand(sql, connection);
        var chars = command.ExecuteScalar();
        print("MySql inited");
    }

    public void Register(string email, string passsword)
    {
        MySqlCommand command;
        StartCoroutine(SqlRegisterRequest());
        IEnumerator SqlRegisterRequest()
        {
            string sql = $"INSERT INTO `users` (`id`, `email`, `password`, `characters`) VALUES (NULL, '{email}', '{passsword}', '{{}}');";
            yield return (command = new MySqlCommand(sql, connection));
            var chars = command.ExecuteScalar();
            if (chars.ToString() != null)
                print(chars.ToString());
        }
    }

    public string Login(string email, string password)
    {
        string characters = null;
        MySqlCommand command;
        StartCoroutine(SqlLoginRequest());
        IEnumerator SqlLoginRequest()
        {
            string sql = $"SELECT characters FROM users WHERE email = '{email}' AND password = '{password}';";
            yield return (command = new MySqlCommand(sql, connection));
            var chars = command.ExecuteScalar();
            if (chars != null)
                characters = chars.ToString();
        }
        return characters;
    }
}
