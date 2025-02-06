
namespace YooAsset
{
    internal enum ERequestStatus
    {
        None,
        InProgress,
        Error,
        Success,
    }

    internal interface IWebRequester
    {
        /// <summary>
        /// 任务状态
        /// </summary>
        public ERequestStatus Status { get; }

        /// <summary>
        /// 下载进度（0f~1f）
        /// </summary>
        public float DownloadProgress { get; }

        /// <summary>
        /// 已经下载的总字节数
        /// </summary>
        public ulong DownloadedBytes { get; }

        /// <summary>
        /// 返回的网络错误
        /// </summary>
        public string RequestNetError { get; }

        /// <summary>
        /// 返回的HTTP CODE
        /// </summary>
        public long RequestHttpCode { get; }


        /// <summary>
        /// 创建任务
        /// </summary>
        public void Create(string url, BundleInfo bundleInfo, params object[] args);

        /// <summary>
        /// 更新任务
        /// </summary>
        public void Update();

        /// <summary>
        /// 终止任务
        /// </summary>
        public void Abort();

        /// <summary>
        /// 是否已经完成（无论成功或失败）
        /// </summary>
        public bool IsDone();

        /// <summary>
        /// 获取请求的对象
        /// </summary>
        public object GetRequestObject();
    }
}