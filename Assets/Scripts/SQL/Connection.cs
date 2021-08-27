using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MySql
{
    public static class Connection
    {
        private const string API_KEY_COOKIE = "AUTH=#i#Li`f2D[W?{$pSL`@4=gvy?1[?gLOyMOUc*TpPdZYKZhj`#%@D^CQ<aR@XwJM";


        public static UnityWebRequest ApiRequest(string request) 
        {
            string uri = "https://irehon.com" + request;
            var www = UnityWebRequest.Get(request);

            www.SetRequestHeader("Cookie", API_KEY_COOKIE);
            
            return www;
        }
    }
}