using System;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace ET.Client
{
    public static partial class HttpClientHelper
    {
        public static async ETTask<string> Get(string link)
        {
            try
            {
#if UNITY
                UnityEngine.Networking.UnityWebRequest req = UnityEngine.Networking.UnityWebRequest.Get(link);
                await req.SendWebRequest();
                return req.downloadHandler.text;
#else
                using HttpClient httpClient = new();
                HttpResponseMessage response =  await httpClient.GetAsync(link);
                string result = await response.Content.ReadAsStringAsync();
                return result;
#endif
            }
            catch (Exception e)
            {
                throw new Exception($"http request fail: {link.Substring(0,link.IndexOf('?'))}\n{e}");
            }
        }
    }
}