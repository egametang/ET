using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine.Networking;
using UnityEngine;

namespace YooAsset
{
    internal abstract class UnityWebRequesterBase
    {
        protected UnityWebRequest _webRequest;
        protected UnityWebRequestAsyncOperation _operationHandle;

        // 超时相关
        private float _timeout;
        private bool _isAbort = false;
        private ulong _latestDownloadBytes;
        private float _latestDownloadRealtime;

        /// <summary>
        /// 请求URL地址
        /// </summary>
        public string URL { protected set; get; }


        protected void ResetTimeout(float timeout)
        {
            _timeout = timeout;
            _latestDownloadBytes = 0;
            _latestDownloadRealtime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// 释放下载器
        /// </summary>
        public void Dispose()
        {
            if (_webRequest != null)
            {
                _webRequest.Dispose();
                _webRequest = null;
                _operationHandle = null;
            }
        }

        /// <summary>
        /// 是否完毕（无论成功失败）
        /// </summary>
        public bool IsDone()
        {
            if (_operationHandle == null)
                return false;
            return _operationHandle.isDone;
        }

        /// <summary>
        /// 下载进度
        /// </summary>
        public float Progress()
        {
            if (_operationHandle == null)
                return 0;
            return _operationHandle.progress;
        }

        /// <summary>
        /// 下载是否发生错误
        /// </summary>
        public bool HasError()
        {
#if UNITY_2020_3_OR_NEWER
            return _webRequest.result != UnityWebRequest.Result.Success;
#else
            if (_webRequest.isNetworkError || _webRequest.isHttpError)
                return true;
            else
                return false;
#endif
        }

        /// <summary>
        /// 获取错误信息
        /// </summary>
        public string GetError()
        {
            if (_webRequest != null)
            {
                return $"URL : {URL} Error : {_webRequest.error}";
            }
            return string.Empty;
        }

        /// <summary>
        /// 检测超时
        /// </summary>
        public void CheckTimeout()
        {
            // 注意：在连续时间段内无新增下载数据及判定为超时
            if (_isAbort == false)
            {
                if (_latestDownloadBytes != _webRequest.downloadedBytes)
                {
                    _latestDownloadBytes = _webRequest.downloadedBytes;
                    _latestDownloadRealtime = Time.realtimeSinceStartup;
                }

                float offset = Time.realtimeSinceStartup - _latestDownloadRealtime;
                if (offset > _timeout)
                {
                    _webRequest.Abort();
                    _isAbort = true;
                }
            }
        }
    }
}