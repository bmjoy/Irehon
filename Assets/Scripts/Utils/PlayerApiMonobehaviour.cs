using Irehon.Client;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Irehon
{
    public class PlayerApiMonobehaviour : MonoBehaviour
    {
        public static PlayerApiMonobehaviour Instance { get; private set; }

        public List<ulong> killQuery;


        private void Awake()
        {
            Instance = this;
        }
        private void Update()
        {
            if (killQuery.Count != 0)
            {
                foreach (ulong id in killQuery.ToArray())
                {
                    var player = ServerManager.Instance.connections.Find(p => p.steamId == id);
                    if (player == null)
                        return;
                    PlayerSession info = (PlayerSession)player.authenticationData;
                    Player spawnedPlayer = info.playerPrefab.GetComponent<Player>();
                    spawnedPlayer.SelfKill();
                    killQuery.Remove(id);
                }
            }
        }
    }
}
