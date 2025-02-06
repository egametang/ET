using UnityEngine;

namespace YooAsset
{
    internal sealed class CompletedDownloader : DownloaderBase
    {
        public CompletedDownloader(BundleInfo bundleInfo) : base(bundleInfo, null, 0, 0)
        {
            DownloadProgress = 1f;
            DownloadedBytes = (ulong)bundleInfo.Bundle.FileSize;
            _status = EStatus.Succeed;
        }

        public override void SendRequest(params object[] param)
        {
        }
        public override void Update()
        {
        }
        public override void Abort()
        {
        }
        public override AssetBundle GetAssetBundle()
        {
            throw new System.NotImplementedException();
        }
    }
}