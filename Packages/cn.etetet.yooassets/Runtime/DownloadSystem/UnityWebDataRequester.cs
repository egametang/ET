using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

namespace YooAsset
{
    internal class UnityWebDataRequester : UnityWebRequesterBase
    {
        /// <summary>
        /// 发送GET请求
        /// </summary>
        public void SendRequest(string url, int timeout = 60)
        {
            if (_webRequest == null)
            {
                URL = url;
                ResetTimeout(timeout);

                _webRequest = DownloadHelper.NewRequest(URL);
                DownloadHandlerBuffer handler = new DownloadHandlerBuffer();
                _webRequest.downloadHandler = handler;
                _webRequest.disposeDownloadHandlerOnDispose = true;
                _operationHandle = _webRequest.SendWebRequest();
            }
        }

        /// <summary>
        /// 获取下载的字节数据
        /// </summary>
        public byte[] GetData()
        {
            if (_webRequest != null && IsDone())
                return _webRequest.downloadHandler.data;
            else
                return null;
        }

        /// <summary>
        /// 获取下载的文本数据
        /// </summary>
        public string GetText()
        {
            if (_webRequest != null && IsDone())
                return _webRequest.downloadHandler.text;
            else
                return null;
        }
    }
}