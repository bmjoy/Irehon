using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Server;
using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace Irehon
{
    public class ServerApi : MonoBehaviour
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
		    // First, we will configure our web server by adding Modules.
                .WithLocalSessionManager()
                .WithModule(new ActionModule("/ping", HttpVerbs.Get, ctx => ctx.SendDataAsync(new { Message = "KEK" })))
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Error" })));

            // Listen for state changes.

            return webserver;
        }
        /*
        [Route(HttpVerbs.Post, "/ping")]
        public string TableTennis()
        {
            return "pong";
        }*/
        private void OnDestroy()
        {
            server.Dispose();
        }

        // Start is called before the first frame update
        void Start()
        {
            StartServerApi();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
