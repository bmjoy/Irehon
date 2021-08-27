using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

namespace MySql
{
    public static class Database
    {
        public const int MAX_CHARACTERS_PER_ACCOUNT = 5;
        public static UnityWebRequest Register(string login, int passsword)
        {
            return Connection.ApiRequest($"/create.player.php?login={login}&password={passsword}");
        }

        public static UnityWebRequest Login(string login, int password)
        {
            return Connection.ApiRequest($"/get.player.php?login={login}&password={password}");
        }

        public static UnityWebRequest CreateNewCharacter(int p_id, string name)
        {
            return Connection.ApiRequest($"/create.character.php?name={name}&id={p_id}");
        }

        public static void DeleteCharacter(int c_id)
        {
            Connection.Delete("characters", "c_id", c_id.ToString());
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

        public static UnityWebRequest GetCharacterId(string name)
        {
            Connection.ApiRequest($"/get.character.name.php?name={name}")
        }
    }
}