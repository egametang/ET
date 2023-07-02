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
                using HttpClient httpClient = new();
                HttpResponseMessage response =  await httpClient.GetAsync(link);
                string result = await response.Content.ReadAsStringAsync();
                return result;
            }
            catch (Exception e)
            {
                throw new Exception($"http request fail: {link.Substring(0,link.IndexOf('?'))}\n{e}");
            }
        }
    }
}