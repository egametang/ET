using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// 自定义下载器的请求委托
    /// </summary>
    public delegate UnityWebRequest DownloadRequestDelegate(string url);


    internal static class DownloadHelper
    {
        /// <summary>
        /// 下载失败后清理文件的HTTP错误码
        /// </summary>
        public static List<long> ClearFileResponseCodes { set; get; }

        /// <summary>
        /// 自定义下载器的请求委托
        /// </summary>
        public static DownloadRequestDelegate RequestDelegate = null;

        /// <summary>
        /// 创建一个新的网络请求
        /// </summary>
        public static UnityWebRequest NewRequest(string requestURL)
        {
            UnityWebRequest webRequest;
            if (RequestDelegate != null)
                webRequest = RequestDelegate.Invoke(requestURL);
            else
                webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbGET);
            return webRequest;
        }
    }
}