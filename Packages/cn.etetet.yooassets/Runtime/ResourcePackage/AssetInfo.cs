
namespace YooAsset
{
    public class AssetInfo
    {
        private readonly PackageAsset _packageAsset;
        private string _providerGUID;

        /// <summary>
        /// 所属包裹
        /// </summary>
        public string PackageName { private set; get; }

        /// <summary>
        /// 资源类型
        /// </summary>
        public System.Type AssetType { private set; get; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { private set; get; }


        /// <summary>
        /// 唯一标识符
        /// </summary>
        internal string GUID
        {
            get
            {
                if (string.IsNullOrEmpty(_providerGUID) == false)
                    return _providerGUID;

                if (AssetType == null)
                    _providerGUID = $"[{AssetPath}][null]";
                else
                    _providerGUID = $"[{AssetPath}][{AssetType.Name}]";
                return _providerGUID;
            }
        }

        /// <summary>
        /// 身份是否无效
        /// </summary>
        internal bool IsInvalid
        {
            get
            {
                return _packageAsset == null;
            }
        }

        /// <summary>
        /// 可寻址地址
        /// </summary>
        public string Address
        {
            get
            {
                if (_packageAsset == null)
                    return string.Empty;
                return _packageAsset.Address;
            }
        }

        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath
        {
            get
            {
                if (_packageAsset == null)
                    return string.Empty;
                return _packageAsset.AssetPath;
            }
        }


        private AssetInfo()
        {
            // 注意：禁止从外部创建该类
        }
        internal AssetInfo(string packageName, PackageAsset packageAsset, System.Type assetType)
        {
            if (packageAsset == null)
                throw new System.Exception("Should never get here !");

            _providerGUID = string.Empty;
            _packageAsset = packageAsset;
            PackageName = packageName;
            AssetType = assetType;
            Error = string.Empty;
        }
        internal AssetInfo(string packageName, string error)
        {
            _providerGUID = string.Empty;
            _packageAsset = null;
            PackageName = packageName;
            AssetType = null;
            Error = error;
        }
    }
}