
namespace YooAsset
{
    public struct DownloadStatus
    {
        /// <summary>
        /// 下载是否完成
        /// </summary>
        public bool IsDone;

        /// <summary>
        /// 下载进度（0f~1f）
        /// </summary>
        public float Progress;

        /// <summary>
        /// 需要下载的总字节数
        /// </summary>
        public ulong TotalBytes;

        /// <summary>
        /// 已经下载的字节数
        /// </summary>
        public ulong DownloadedBytes;

        public static DownloadStatus CreateDefaultStatus()
        {
            DownloadStatus status = new DownloadStatus();
            status.IsDone = false;
            status.Progress = 0f;
            status.TotalBytes = 0;
            status.DownloadedBytes = 0;
            return status;
        }
    }
}