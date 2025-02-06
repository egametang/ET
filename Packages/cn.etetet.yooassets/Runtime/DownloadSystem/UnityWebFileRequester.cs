using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

namespace YooAsset
{
    internal class UnityWebFileRequester : UnityWebRequesterBase
    {
        /// <summary>
        /// 发送GET请求
        /// </summary>
        public void SendRequest(string url, string fileSavePath, int timeout = 60)
        {
            if (_webRequest == null)
            {
                URL = url;
                ResetTimeout(timeout);

                _webRequest = DownloadHelper.NewRequest(URL);
                DownloadHandlerFile handler = new DownloadHandlerFile(fileSavePath);
                handler.removeFileOnAbort = true;
                _webRequest.downloadHandler = handler;
                _webRequest.disposeDownloadHandlerOnDispose = true;
                _operationHandle = _webRequest.SendWebRequest();
            }
        }
    }
}