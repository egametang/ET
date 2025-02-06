using System.IO;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 解密文件的信息
    /// </summary>
    public struct DecryptFileInfo
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
    }

    /// <summary>
    /// 解密类服务接口
    /// </summary>
    public interface IDecryptionServices
    {
        /// <summary>
        /// 同步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        AssetBundle LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream);

        /// <summary>
        /// 异步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        AssetBundleCreateRequest LoadAssetBundleAsync(DecryptFileInfo fileInfo, out Stream managedStream);
    }
}