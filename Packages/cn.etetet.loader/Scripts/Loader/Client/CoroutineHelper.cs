using System;
using UnityEngine;
using UnityEngine.Networking;

namespace ET
{
    public static class CoroutineHelper
    {
        // 有了这个方法，就可以直接await Unity的AsyncOperation了
        public static async ETTask GetAwaiter(this AsyncOperation asyncOperation)
        {
            ETTask task = ETTask.Create(true);
            asyncOperation.completed += _ => { task.SetResult(); };
            await task;
        }
        
        public static async ETTask<string> HttpGet(string link)
        {
            try
            {
                UnityWebRequest req = UnityWebRequest.Get(link);
                await req.SendWebRequest();
                return req.downloadHandler.text;
            }
            catch (Exception e)
            {
                throw new Exception($"http request fail: {link.Substring(0,link.IndexOf('?'))}\n{e}");
            }
        }
    }
}