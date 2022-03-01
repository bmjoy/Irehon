using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Irehon;
using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System.Diagnostics;
using SimpleJSON;
using Irehon.Client;

namespace Irehon
{
    public class WebserverApiHolder : MonoBehaviour
    {
        private static string url = "http://localhost:8283/";

        private WebServer server = CreateWebServer();
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private void StartServerApi()
        {
            server.RunAsync();
        }

        // Create and configure our web server.
        private static WebServer CreateWebServer()
        {
            var webserver = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
                    .WithLocalSessionManager()
                        .WithWebApi("/", m => m
                        .WithController<PlayerApiController>()
                        .WithController<ServerApiController>());

            // Listen for state changes.

            return webserver;
        }
        private void OnDestroy()
        {
            server.Dispose();
        }

        // Start is called before the first frame update
        void Start()
        {
            StartServerApi();
        }
    }

    public class ServerApiController : WebApiController
    {
        [Route(HttpVerbs.Post, "/whitelist/{mode}")]
        public string ChangeWhitelistMode(string mode)
        {
            if (mode == "on")
            {
                ServerManager.Instance.isWhiteListEnabled = true;
                return "Enabled";
            }
            else if (mode == "off")
            {
                ServerManager.Instance.isWhiteListEnabled = false;
                return "Disabled";
            }
            return $"unkown mode. Got {mode}. \"on\" or \"off\"";
        }

        [Route(HttpVerbs.Post, "/disable")]
        public async Task<string> DisableServer()
        {
            ServerApiMonobehaviour.Instance.isServerShouldStop = true;
            await Task.Delay(2000);
            return "disabling";
        }
    }

    public class PlayerApiController : WebApiController
    {
        [Route(HttpVerbs.Get, "/players")]
        public IEnumerable<string> GetPlayers()
        {
            var players = ServerManager.Instance.connections;
            List<string> connectedPlayersId = new List<string>();
            foreach (var player in players)
            {
                if (!player.isAuthenticated)
                    connectedPlayersId.Add("Unauthorized");
                var id = player.steamId;
                connectedPlayersId.Add(id.ToString());
            }
            return connectedPlayersId;
        }

        [Route(HttpVerbs.Post, "/kick/{id}")]
        public string GetPlayerCount(ulong id)
        {
            var player = ServerManager.Instance.connections.Find(p => p.steamId == id);
            if (player == null)
            {
                return "Player not found";
            }
            else
            {
                ServerApiMonobehaviour.Instance.kickQuery.Add(id);

                return "Kicked";
            }
        }

        [Route(HttpVerbs.Get, "/count")]
        public string GetPlayerCount()
        {
            return ServerManager.Instance.connections.Count.ToString();
        }
    }
}
