using System.IO;
using UnityEngine.Networking;

namespace YooAsset
{
    internal class FileGeneralRequest : IWebRequester
    {
        private UnityWebRequest _webRequest;

        public ERequestStatus Status { private set; get; } = ERequestStatus.None;
        public float DownloadProgress { private set; get; }
        public ulong DownloadedBytes { private set; get; }
        public string RequestNetError { private set; get; }
        public long RequestHttpCode { private set; get; }

        public FileGeneralRequest() { }
        public void Create(string requestURL, BundleInfo bundleInfo, params object[] args)
        {
            if (Status != ERequestStatus.None)
                throw new System.Exception("Should never get here !");

            string tempFilePath = bundleInfo.TempDataFilePath;

            // 删除临时文件
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);

            // 创建下载器
            _webRequest = DownloadHelper.NewRequest(requestURL);
            DownloadHandlerFile handler = new DownloadHandlerFile(tempFilePath);
            handler.removeFileOnAbort = true;
            _webRequest.downloadHandler = handler;
            _webRequest.disposeDownloadHandlerOnDispose = true;
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
            DownloadedBytes = _webRequest.downloadedBytes;
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
            if (_webRequest != null)
            {
                _webRequest.Dispose(); //注意：引擎底层会自动调用Abort方法
                _webRequest = null;
            }
        }
    }
}