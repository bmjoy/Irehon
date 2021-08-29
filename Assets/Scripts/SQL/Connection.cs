using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;



public enum ApiMethod { GET, PUT, POST, DELETE };

namespace Deprecated
{
    public enum ApiMethod { GET, POST, PUT, DELETE };
    public static class Connection
    {
        private const string API_KEY_COOKIE = "AUTH=#i#Li`f2D[W?{$pSL`@4=gvy?1[?gLOyMOUc*TpPdZYKZhj`#%@D^CQ<aR@XwJM";
        public static IEnumerator Request(string request, ApiMethod method = ApiMethod.GET) 
        {
            string uri = "https://irehon.com/api" + request;
            var www = new UnityWebRequest(request);
            www.method = method.ToString();
            www.SetRequestHeader("Cookie", API_KEY_COOKIE);

            yield return www.SendWebRequest();


        }
    }
}