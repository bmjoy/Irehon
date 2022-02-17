using SimpleJSON;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Irehon.CloudAPI
{
    public enum ApiMethod { GET, POST, PUT, DELETE };
    public static class Api
    {
        private const string API_KEY_COOKIE = "AUTH=#i#Li`f2D[W?{$pSL`@4=gvy?1[?gLOyMOUc*TpPdIPHBUJKPUIPYFG&FUGI(FHFG&*FGUIF&IGFYU&FIGUYILFUZYKZhj`#%@D^CQ<aR@XwJM";


        public static UnityWebRequest Request(string request, ApiMethod method = ApiMethod.GET)
        {
            string uri = "https://api.irehon.com/api" + request;
            var www = new UnityWebRequest(uri);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.method = method.ToString();
            www.SetRequestHeader("Cookie", API_KEY_COOKIE);

            return www;
        }

        public static UnityWebRequest SqlRequest(string request, ApiMethod method = ApiMethod.GET)
        {
            string uri = "https://api.irehon.com" + request;
            var www = new UnityWebRequest(uri);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.method = method.ToString();
            www.SetRequestHeader("Cookie", API_KEY_COOKIE);

            return www;
        }

        public static JSONNode GetResult(UnityWebRequest request)
        {
            if (request.responseCode == 200)
            {
                return JSON.Parse(request.downloadHandler.text);
            }
            else
            {
                return null;
            }
        }
    }
}