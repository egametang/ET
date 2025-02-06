using System;
using System.Linq;

namespace YooAsset
{
    [Serializable]
    internal class PackageBundle
    {
        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName;

        /// <summary>
        /// Unity引擎生成的CRC
        /// </summary>
        public uint UnityCRC;

        /// <summary>
        /// 文件哈希值
        /// </summary>
        public string FileHash;

        /// <summary>
        /// 文件校验码
        /// </summary>
        public string FileCRC;

        /// <summary>
        /// 文件大小（字节数）
        /// </summary>
        public long FileSize;

        /// <summary>
        /// 文件是否加密
        /// </summary>
        public bool Encrypted;

        /// <summary>
        /// 资源包的分类标签
        /// </summary>
        public string[] Tags;

        /// <summary>
        /// 依赖的资源包ID集合
        /// </summary>
        public int[] DependIDs;


        /// <summary>
        /// 所属的包裹名称
        /// </summary>
        public string PackageName { private set; get; }

        /// <summary>
        /// 所属的构建管线
        /// </summary>
        public string Buildpipeline { private set; get; }

        /// <summary>
        /// 缓存GUID
        /// </summary>
        public string CacheGUID
        {
            get { return FileHash; }
        }

        /// <summary>
        /// 文件名称
        /// </summary>
        private string _fileName;
        public string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(_fileName))
                    throw new Exception("Should never get here !");
                return _fileName;
            }
        }

        /// <summary>
        /// 文件后缀名
        /// </summary>
        private string _fileExtension;
        public string FileExtension
        {
            get
            {
                if (string.IsNullOrEmpty(_fileExtension))
                    throw new Exception("Should never get here !");
                return _fileExtension;
            }
        }


        public PackageBundle()
        {
        }

        /// <summary>
        /// 解析资源包
        /// </summary>
        public void ParseBundle(PackageManifest manifest)
        {
            PackageName = manifest.PackageName;
            Buildpipeline = manifest.BuildPipeline;
            _fileExtension = ManifestTools.GetRemoteBundleFileExtension(BundleName);
            _fileName = ManifestTools.GetRemoteBundleFileName(manifest.OutputNameStyle, BundleName, _fileExtension, FileHash);
        }

        /// <summary>
        /// 是否包含Tag
        /// </summary>
        public bool HasTag(string[] tags)
        {
            if (tags == null || tags.Length == 0)
                return false;
            if (Tags == null || Tags.Length == 0)
                return false;

            foreach (var tag in tags)
            {
                if (Tags.Contains(tag))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 是否包含任意Tags
        /// </summary>
        public bool HasAnyTags()
        {
            if (Tags != null && Tags.Length > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 检测资源包文件内容是否相同
        /// </summary>
        public bool Equals(PackageBundle otherBundle)
        {
            if (FileHash == otherBundle.FileHash)
                return true;

            return false;
        }
    }
}