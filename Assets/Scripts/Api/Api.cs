using SimpleJSON;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Irehon.CloudAPI
{
    public enum ApiMethod { GET, POST, PUT, DELETE };
    public static class Api
    {
        private const string API_KEY_COOKIE = "AUTH=B!A@SF#AD#F&DAB&VFA&DAHDFODGJIOOOFGDKOHUIHYQEBNVVCIO#3414ODKB4451PPIGP88519876j15hiUJ";


        public static UnityWebRequest Request(string request, ApiMethod method = ApiMethod.GET, string body = null)
        {
            string uri = "https://testapi.irehon.com" + request;
            UnityWebRequest www;
            if (body != null && body != "")
            {
                www = UnityWebRequest.Put(uri, body);
                www.method = method.ToString();
            }
            else
                www = new UnityWebRequest(uri);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.method = method.ToString();
            www.SetRequestHeader("Cookie", API_KEY_COOKIE);

            return www;
        }

        public static JSONNode GetResult(UnityWebRequest request)
        {
            Debug.Log($"Request: {request.uri} {request.method} {request.responseCode} {request.downloadHandler.text}");
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