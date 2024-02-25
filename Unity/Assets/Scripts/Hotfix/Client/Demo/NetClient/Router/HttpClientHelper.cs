using System;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace ET.Client
{
    public static partial class HttpClientHelper
    {
        public static async ETTask<(bool,string)> Get(string link)
        {
            try
            {
                using HttpClient httpClient = new();
                HttpResponseMessage response =  await httpClient.GetAsync(link);
                string result = await response.Content.ReadAsStringAsync();
                return (true,result);
            }
            catch (Exception e)
            {
                Log.Error($"Http请求失败: {link.Substring(0,link.IndexOf('?'))}\n{e}");
                return (false, null);
            }
        }
    }
}