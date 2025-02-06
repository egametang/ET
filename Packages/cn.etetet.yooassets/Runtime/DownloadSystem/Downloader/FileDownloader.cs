using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 文件下载器
    /// </summary>
    internal sealed class FileDownloader : DownloaderBase
    {
        private enum ESteps
        {
            None,
            PrepareDownload,
            CreateDownloader,
            CheckDownload,
            VerifyTempFile,
            WaitingVerifyTempFile,
            CachingFile,
            TryAgain,
            Done,
        }

        private VerifyTempFileOperation _verifyFileOp = null;
        private ESteps _steps = ESteps.None;

        public FileDownloader(BundleInfo bundleInfo, System.Type requesterType, int failedTryAgain, int timeout) : base(bundleInfo, requesterType, failedTryAgain, timeout)
        {
        }
        public override void SendRequest(params object[] args)
        {
            if (_steps == ESteps.None)
            {
                _steps = ESteps.PrepareDownload;
            }
        }
        public override void Update()
        {
            if (_steps == ESteps.None)
                return;
            if (IsDone())
                return;

            // 准备下载
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
                _requester.Create(_requestURL, _bundleInfo);
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
                    _steps = ESteps.VerifyTempFile;
                }
            }

            // 验证下载文件
            if (_steps == ESteps.VerifyTempFile)
            {
                VerifyTempFileElement element = new VerifyTempFileElement(_bundleInfo.TempDataFilePath, _bundleInfo.Bundle.FileCRC, _bundleInfo.Bundle.FileSize);
                _verifyFileOp = VerifyTempFileOperation.CreateOperation(element);
                OperationSystem.StartOperation(_bundleInfo.Bundle.PackageName, _verifyFileOp);
                _steps = ESteps.WaitingVerifyTempFile;
            }

            // 等待验证完成
            if (_steps == ESteps.WaitingVerifyTempFile)
            {
                if (WaitForAsyncComplete)
                    _verifyFileOp.InternalOnUpdate();

                if (_verifyFileOp.IsDone == false)
                    return;

                if (_verifyFileOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.CachingFile;
                }
                else
                {
                    string tempFilePath = _bundleInfo.TempDataFilePath;
                    if (File.Exists(tempFilePath))
                        File.Delete(tempFilePath);

                    _lastestNetError = _verifyFileOp.Error;
                    _steps = ESteps.TryAgain;
                }
            }

            // 缓存下载文件
            if (_steps == ESteps.CachingFile)
            {
                try
                {
                    CachingFile();
                    _status = EStatus.Succeed;
                    _steps = ESteps.Done;
                }
                catch (Exception e)
                {
                    _lastestNetError = e.Message;
                    _steps = ESteps.TryAgain;
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// 缓存下载文件
        /// </summary>
        private void CachingFile()
        {
            string tempFilePath = _bundleInfo.TempDataFilePath;
            string infoFilePath = _bundleInfo.CachedInfoFilePath;
            string dataFilePath = _bundleInfo.CachedDataFilePath;
            string dataFileCRC = _bundleInfo.Bundle.FileCRC;
            long dataFileSize = _bundleInfo.Bundle.FileSize;

            if (File.Exists(infoFilePath))
                File.Delete(infoFilePath);
            if (File.Exists(dataFilePath))
                File.Delete(dataFilePath);

            // 移动临时文件路径
            FileInfo fileInfo = new FileInfo(tempFilePath);
            fileInfo.MoveTo(dataFilePath);

            // 写入信息文件记录验证数据
            CacheFileInfo.WriteInfoToFile(infoFilePath, dataFileCRC, dataFileSize);

            // 记录缓存文件
            _bundleInfo.CacheRecord();
        }
    }
}