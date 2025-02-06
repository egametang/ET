
namespace YooAsset
{
    public struct EncryptResult
    {
        /// <summary>
        /// 文件是否加密
        /// </summary>
        public bool Encrypted;

        /// <summary>
        /// 加密后的文件数据
        /// </summary>
        public byte[] EncryptedData;
    }

    public struct EncryptFileInfo
    {
        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName;

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath;
    }

    /// <summary>
    /// 加密服务类接口
    /// </summary>
    public interface IEncryptionServices
    {
        EncryptResult Encrypt(EncryptFileInfo fileInfo);
    }
}