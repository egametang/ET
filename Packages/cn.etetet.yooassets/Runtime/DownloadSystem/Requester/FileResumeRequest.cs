using System.IO;
using UnityEngine.Networking;

namespace YooAsset
{
    internal class FileResumeRequest : IWebRequester
    {
        private string _tempFilePath;
        private UnityWebRequest _webRequest;
        private DownloadHandlerFileRange _downloadHandle;
        private ulong _fileOriginLength = 0;

        public ERequestStatus Status { private set; get; } = ERequestStatus.None;
        public float DownloadProgress { private set; get; }
        public ulong DownloadedBytes { private set; get; }
        public string RequestNetError { private set; get; }
        public long RequestHttpCode { private set; get; }

        public FileResumeRequest() { }
        public void Create(string requestURL, BundleInfo bundleInfo, params object[] args)
        {
            if (Status != ERequestStatus.None)
                throw new System.Exception("Should never get here !");

            _tempFilePath = bundleInfo.TempDataFilePath;
            long fileBytes = bundleInfo.Bundle.FileSize;

            // 获取下载的起始位置
            long fileLength = -1;
            if (File.Exists(_tempFilePath))
            {
                FileInfo fileInfo = new FileInfo(_tempFilePath);
                fileLength = fileInfo.Length;
                _fileOriginLength = (ulong)fileLength;
                DownloadedBytes = _fileOriginLength;
            }

            // 检测下载起始位置是否有效
            if (fileLength >= fileBytes)
            {
                if (File.Exists(_tempFilePath))
                    File.Delete(_tempFilePath);
            }

            // 创建下载器
            _webRequest = DownloadHelper.NewRequest(requestURL);
#if UNITY_2019_4_OR_NEWER
            var handler = new DownloadHandlerFile(_tempFilePath, true);
            handler.removeFileOnAbort = false;
#else
            var handler = new DownloadHandlerFileRange(tempFilePath, _bundleInfo.Bundle.FileSize, _webRequest);
            _downloadHandle = handler;
#endif
            _webRequest.downloadHandler = handler;
            _webRequest.disposeDownloadHandlerOnDispose = true;
            if (fileLength > 0)
                _webRequest.SetRequestHeader("Range", $"bytes={fileLength}-");
            _webRequest.SendWebRequest();
            Status = ERequestStatus.InProgress;
        }
        public void Update()
        {
            if (Status == ERequestStatus.None)
                return;
            if (IsDone())
                return;

            DownloadProgress = _webRequest.downloadProgress;
            DownloadedBytes = _fileOriginLength + _webRequest.downloadedBytes;
            if (_webRequest.isDone == false)
                return;

            // 检查网络错误
#if UNITY_2020_3_OR_NEWER
            RequestHttpCode = _webRequest.responseCode;
            if (_webRequest.result != UnityWebRequest.Result.Success)
            {
                RequestNetError = _webRequest.error;
                Status = ERequestStatus.Error;
            }
            else
            {
                Status = ERequestStatus.Success;
            }
#else
            RequestHttpCode = _webRequest.responseCode;
            if (_webRequest.isNetworkError || _webRequest.isHttpError)
            {
                RequestNetError = _webRequest.error;
                Status = ERequestStatus.Error;
            }
            else
            {
                Status = ERequestStatus.Success;
            }
#endif

            // 注意：下载断点续传文件发生特殊错误码之后删除文件
            if (Status == ERequestStatus.Error)
            {
                if (DownloadHelper.ClearFileResponseCodes != null)
                {
                    if (DownloadHelper.ClearFileResponseCodes.Contains(RequestHttpCode))
                    {
                        if (File.Exists(_tempFilePath))
                            File.Delete(_tempFilePath);
                    }
                }
            }

            // 最终释放下载器
            DisposeWebRequest();
        }
        public void Abort()
        {
            DisposeWebRequest();
            if (IsDone() == false)
            {
                RequestNetError = "user abort";
                RequestHttpCode = 0;
                Status = ERequestStatus.Error;
            }
        }
        public bool IsDone()
        {
            if (Status == ERequestStatus.Success || Status == ERequestStatus.Error)
                return true;
            else
                return false;
        }
        public object GetRequestObject()
        {
            throw new System.NotImplementedException();
        }
        private void DisposeWebRequest()
        {
            if (_downloadHandle != null)
            {
                _downloadHandle.Cleanup();
                _downloadHandle = null;
            }

            if (_webRequest != null)
            {
                _webRequest.Dispose();
                _webRequest = null;
            }
        }
    }
}