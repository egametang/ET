using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 分发的资源信息
    /// </summary>
    public struct DeliveryFileInfo
    {
        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName;

        /// <summary>
        /// 文件加载路径
        /// </summary>
        public string FileLoadPath;

        /// <summary>
        /// Unity引擎用于内容校验的CRC
        /// </summary>
        public uint ConentCRC;

        /// <summary>
        /// 资源包是否加密
        /// </summary>
        public bool Encrypted;
    }

    public interface IDeliveryLoadServices
    {
        /// <summary>
        /// 同步方式获取分发的资源包对象
        /// </summary>
        AssetBundle LoadAssetBundle(DeliveryFileInfo fileInfo);

        /// <summary>
        /// 异步方式获取分发的资源包对象
        /// </summary>
        AssetBundleCreateRequest LoadAssetBundleAsync(DeliveryFileInfo fileInfo);
    }
}