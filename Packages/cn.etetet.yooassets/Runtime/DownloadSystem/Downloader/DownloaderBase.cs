using System;
using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
    internal abstract class DownloaderBase
    {
        public enum EStatus
        {
            None = 0,
            Succeed,
            Failed
        }

        protected readonly BundleInfo _bundleInfo;
        protected readonly System.Type _requesterType;
        protected readonly int _timeout;
        protected int _failedTryAgain;

        protected IWebRequester _requester;
        protected EStatus _status = EStatus.None;
        protected string _lastestNetError = string.Empty;
        protected long _lastestHttpCode = 0;

        // 请求次数
        protected int _requestCount = 0;
        protected string _requestURL;

        // 超时相关
        protected bool _isAbort = false;
        protected ulong _latestDownloadBytes;
        protected float _latestDownloadRealtime;
        protected float _tryAgainTimer;

        /// <summary>
        /// 是否等待异步结束
        /// 警告：只能用于解压APP内部资源
        /// </summary>
        public bool WaitForAsyncComplete = false;

        /// <summary>
        /// 下载进度（0f~1f）
        /// </summary>
        public float DownloadProgress { protected set; get; }

        /// <summary>
        /// 已经下载的总字节数
        /// </summary>
        public ulong DownloadedBytes { protected set; get; }

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { private set; get; }


        public DownloaderBase(BundleInfo bundleInfo, System.Type requesterType, int failedTryAgain, int timeout)
        {
            _bundleInfo = bundleInfo;
            _requesterType = requesterType;
            _failedTryAgain = failedTryAgain;
            _timeout = timeout;
        }
        public abstract void SendRequest(params object[] args);
        public abstract void Update();
        public abstract void Abort();
        public abstract AssetBundle GetAssetBundle();

        /// <summary>
        /// 引用（引用计数递加）
        /// </summary>
        public void Reference()
        {
            RefCount++;
        }

        /// <summary>
        /// 释放（引用计数递减）
        /// </summary>
        public void Release()
        {
            RefCount--;
        }

        /// <summary>
        /// 检测下载器是否已经完成（无论成功或失败）
        /// </summary>
        public bool IsDone()
        {
            return _status == EStatus.Succeed || _status == EStatus.Failed;
        }

        /// <summary>
        /// 下载过程是否发生错误
        /// </summary>
        public bool HasError()
        {
            return _status == EStatus.Failed;
        }

        /// <summary>
        /// 按照错误级别打印错误
        /// </summary>
        public void ReportError()
        {
            YooLogger.Error(GetLastError());
        }

        /// <summary>
        /// 按照警告级别打印错误
        /// </summary>
        public void ReportWarning()
        {
            YooLogger.Warning(GetLastError());
        }

        /// <summary>
        /// 获取最近发生的错误信息
        /// </summary>
        public string GetLastError()
        {
            return $"Failed to download : {_requestURL} Error : {_lastestNetError} Code : {_lastestHttpCode}";
        }

        /// <summary>
        /// 获取下载文件的大小
        /// </summary>
        /// <returns></returns>
        public long GetDownloadFileSize()
        {
            return _bundleInfo.Bundle.FileSize;
        }

        /// <summary>
        /// 获取下载的资源包名称
        /// </summary>
        public string GetDownloadBundleName()
        {
            return _bundleInfo.Bundle.BundleName;
        }


        /// <summary>
        /// 获取网络请求地址
        /// </summary>
        protected string GetRequestURL()
        {
            // 轮流返回请求地址
            _requestCount++;
            if (_requestCount % 2 == 0)
                return _bundleInfo.RemoteFallbackURL;
            else
                return _bundleInfo.RemoteMainURL;
        }

        /// <summary>
        /// 超时判定方法
        /// </summary>
        protected void CheckTimeout()
        {
            // 注意：在连续时间段内无新增下载数据及判定为超时
            if (_isAbort == false)
            {
                if (_latestDownloadBytes != DownloadedBytes)
                {
                    _latestDownloadBytes = DownloadedBytes;
                    _latestDownloadRealtime = Time.realtimeSinceStartup;
                }

                float offset = Time.realtimeSinceStartup - _latestDownloadRealtime;
                if (offset > _timeout)
                {
                    YooLogger.Warning($"Web file request timeout : {_requestURL}");
                    if (_requester != null)
                        _requester.Abort();
                    _isAbort = true;
                }
            }
        }
    }
}