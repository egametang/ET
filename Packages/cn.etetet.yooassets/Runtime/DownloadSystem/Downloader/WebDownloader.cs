using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YooAsset
{
    internal sealed class WebDownloader : DownloaderBase
    {
        private enum ESteps
        {
            None,
            PrepareDownload,
            CreateDownloader,
            CheckDownload,
            TryAgain,
            Done,
        }

        private ESteps _steps = ESteps.None;
        private bool _getAssetBundle = false;

        public WebDownloader(BundleInfo bundleInfo, System.Type requesterType, int failedTryAgain, int timeout) : base(bundleInfo, requesterType, failedTryAgain, timeout)
        {
        }
        public override void SendRequest(params object[] args)
        {
            if (_steps == ESteps.None)
            {
                if (args.Length > 0)
                {
                    _getAssetBundle = (bool)args[0];
                }
                _steps = ESteps.PrepareDownload;
            }
        }
        public override void Update()
        {
            if (_steps == ESteps.None)
                return;
            if (IsDone())
                return;

            // 创建下载器
            if (_steps == ESteps.PrepareDownload)
            {
                // 获取请求地址
                _requestURL = GetRequestURL();

                // 重置变量
                DownloadProgress = 0f;
                DownloadedBytes = 0;

                // 重置变量
                _isAbort = false;
                _latestDownloadBytes = 0;
                _latestDownloadRealtime = Time.realtimeSinceStartup;

                // 重置计时器
                if (_tryAgainTimer > 0f)
                    YooLogger.Warning($"Try again download : {_requestURL}");
                _tryAgainTimer = 0f;

                _steps = ESteps.CreateDownloader;
            }

            // 创建下载器
            if (_steps == ESteps.CreateDownloader)
            {
                _requester = (IWebRequester)Activator.CreateInstance(_requesterType);
                _requester.Create(_requestURL, _bundleInfo, _getAssetBundle);
                _steps = ESteps.CheckDownload;
            }

            // 检测下载结果
            if (_steps == ESteps.CheckDownload)
            {
                _requester.Update();
                DownloadedBytes = _requester.DownloadedBytes;
                DownloadProgress = _requester.DownloadProgress;
                if (_requester.IsDone() == false)
                {
                    CheckTimeout();
                    return;
                }

                _lastestNetError = _requester.RequestNetError;
                _lastestHttpCode = _requester.RequestHttpCode;
                if (_requester.Status != ERequestStatus.Success)
                {
                    _steps = ESteps.TryAgain;
                }
                else
                {
                    _status = EStatus.Succeed;
                    _steps = ESteps.Done;
                }
            }

            // 重新尝试下载
            if (_steps == ESteps.TryAgain)
            {
                if (_failedTryAgain <= 0)
                {
                    ReportError();
                    _status = EStatus.Failed;
                    _steps = ESteps.Done;
                    return;
                }

                _tryAgainTimer += Time.unscaledDeltaTime;
                if (_tryAgainTimer > 1f)
                {
                    _failedTryAgain--;
                    _steps = ESteps.PrepareDownload;
                    ReportWarning();
                    YooLogger.Warning($"Try again download : {_requestURL}");
                }
            }
        }
        public override void Abort()
        {
            if (_requester != null)
                _requester.Abort();

            if (IsDone() == false)
            {
                _status = EStatus.Failed;
                _steps = ESteps.Done;
                _lastestNetError = "user abort";
                _lastestHttpCode = 0;
            }
        }
        public override AssetBundle GetAssetBundle()
        {
            return (AssetBundle)_requester.GetRequestObject();
        }
    }
}