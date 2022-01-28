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

        private static async Task<float> GetCpuUsageForProcess()
        {
            var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue();
            cpuCounter.NextValue();
            await Task.Delay(500);
            return cpuCounter.NextValue();
        }

        // Create and configure our web server.
        private static WebServer CreateWebServer()
        {
            var webserver = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
		    // First, we will configure our web server by adding Modules.
                .WithLocalSessionManager()
                .WithModule(new ActionModule("/cpu", HttpVerbs.Get, async ctx => ctx.SendDataAsync(new { Cpu = await GetCpuUsageForProcess()})))
                .WithModule(new ActionModule("/players", HttpVerbs.Get, ctx => ctx.SendDataAsync(new { Online = ServerManager.Instance.connections.Count })))
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
        float timer;
        PerformanceCounter cpuUsage = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        void Update()
        {
            timer += Time.deltaTime;
            if (timer > 1)
            {
                print(cpuUsage.NextValue() + "%");
                timer = 0;
            }
        }
    }
}
