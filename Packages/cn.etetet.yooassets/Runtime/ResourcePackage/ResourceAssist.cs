
namespace YooAsset
{
    internal class ResourceAssist
    {
        public CacheManager Cache { set; get; }
        public PersistentManager Persistent { set; get; }
        public DownloadManager Download { set; get; }
        public ResourceLoader Loader { set; get; }
    }
}