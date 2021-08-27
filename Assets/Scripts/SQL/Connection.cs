using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace MySql
{
    public class Querry
    {
        private string command;
        public string Run()
        {
            return Connection.RecieveSingleData(command);
        }
        public Querry Insert(string table, string key, string value)
        {
            command += $"INSERT INTO {table} ({key}) VALUES ('{value}'); SELECT LAST_INSERT_ID();";

            return this;
        }

        public Querry Insert(string table, Dictionary<string, string> values)
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
            this.command += command;
            return this;
        }

        public Querry UpdateColumn(string table, string filter, string filterValue, Dictionary<string, string> updateValues)
        {
            string command = $"UPDATE {table} SET ";
            for (int i = 0; i < updateValues.Count; i++)
            {
                if (i != 0)
                    command += ", ";
                var valuePair = updateValues.ElementAt(i);
                command += $"{valuePair.Key} = '{valuePair.Value}'";
            }
            command += $" WHERE {filter} = '{filterValue}';";
            this.command += command;
            return this;
        }

        public Querry UpdateColumn(string table, string filter, string filterValue, string valueKey, string value)
        {
            command += $"UPDATE {table} SET {valueKey} = '{value}' WHERE {filter} = '{filterValue}';";
            return this;
        }

        public Querry Delete(string table, string filter, string value)
        {
            command += $"DELETE FROM {table} WHERE {filter} = '{value}';";
            return this;
        }
    }
    public static class Connection
    {
        private static MySqlConnection connection;

        static Connection()
        {
            connection = new MySqlConnection("server = 134.209.21.121; " +
                "user = server; database = players; password = Q@!#AFZDZDF!AASDS;");
            connection.Open();
            RecieveSingleData("USE players;");
        }

        public static void Shutdown()
        {
            connection.Close();
        }

        public static string RecieveJson(string tableName, string[] columns)
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
            Debug.Log(recieve);
            return recieve;
        }

        public static List<string> RecieveMultipleData(string command, int columnQuantity)
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

        public static string UpdateColumn(string table, string filter, string filterValue, Dictionary<string, string> updateValues)
        {
            string command = $"UPDATE {table} SET ";
            for (int i = 0; i < updateValues.Count; i++)
            {
                if (i != 0)
                    command += ", ";
                var valuePair = updateValues.ElementAt(i);
                command += $"{valuePair.Key} = '{valuePair.Value}'";
            }
            command += $" WHERE {filter} = '{filterValue}';";
            return RecieveSingleData(command);
        }

        public static string UpdateColumn(string table, string filter, string filterValue, string valueKey, string value)
        {
            string command = $"UPDATE {table} SET {valueKey} = '{value}' WHERE {filter} = '{filterValue}';";
            return RecieveSingleData(command);
        }

        public static List<string> MultipleSelect(string table, List<string> columns, string valueKey, string value)
        {
            string command = "SELECT ";
            for (int i = 0; i < columns.Count; i++)
            {
                if (i != 0)
                    command += ", ";
                command += columns[i];
            }
            command += $" FROM {table} WHERE {valueKey} = '{value}';";
            return RecieveMultipleData(command, columns.Count);
        }

        public static List<string> MultipleSelect(string table, List<string> columns, Dictionary<string, string> filter)
        {
            string command = "SELECT ";
            for (int i = 0; i < columns.Count; i++)
            {
                if (i != 0)
                    command += ", ";
                command += columns[i];
            }
            command += " FROM " + table;
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
            return RecieveMultipleData(command, columns.Count);
        }

        public static string SingleSelect(string table, string column, Dictionary<string, string> filter)
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

        public static string SingleSelect(string table, string column, string filter, string value)
        {
            string command = $"SELECT {column} FROM {table}";
            command += $" WHERE {filter} = '{value}';";
            return RecieveSingleData(command);
        }

        public static Vector3 GetVector3(string sqlCommand)
        {
            Vector3 vector = Vector3.zero;

            List<string> positionPoints = RecieveMultipleData(sqlCommand, 3);

            vector.x = (float)Convert.ToDouble(positionPoints[0]);
            vector.y = (float)Convert.ToDouble(positionPoints[1]);
            vector.z = (float)Convert.ToDouble(positionPoints[2]);

            return vector;
        }

        public static string Insert(string table, string key, string value)
        {
            return RecieveSingleData($"INSERT INTO {table} ({key}) VALUES ('{value}'); SELECT LAST_INSERT_ID();");
        }

        public static string Insert(string table, Dictionary<string, string> values)
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

        public static string Delete(string table, string filter, string value)
        {
            string command = $"DELETE FROM {table} WHERE {filter} = '{value}';";
            return RecieveSingleData(command);
        }

        public static string RecieveSingleData(string command)
        {
            MySqlCommand commandResponse = new MySqlCommand(command, connection);
            var chars = commandResponse.ExecuteScalar();
            if (chars != null)
                return chars.ToString();
            else
                return null;
        }
    }
}