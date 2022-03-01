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
    public class ServerApiMonobehaviour : MonoBehaviour
    {
        public static ServerApiMonobehaviour Instance { get; private set; }

        public List<ulong> kickQuery;
        public bool isServerShouldStop;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (kickQuery.Count != 0)
            {
                foreach (ulong id in kickQuery.ToArray())
                {
                    var player = ServerManager.Instance.connections.Find(p => p.steamId == id);
                    if (player == null)
                        return;
                    ServerManager.SendMessage(player, "You've been kicked. ", MessageType.Notification);
                    ServerManager.DisconnectWithDelay(player);
                    kickQuery.Remove(id);
                }
            }
            if (isServerShouldStop)
            {
                ServerManager.Instance.ManuallyStopServer();
                isServerShouldStop = false;
            }
        }
    }
}
