
namespace YooAsset
{
	public struct DecryptFileInfo
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
	/// 解密类服务接口
	/// </summary>
	public interface IDecryptionServices
	{
		/// <summary>
		/// 文件偏移解密方法
		/// </summary>
		ulong LoadFromFileOffset(DecryptFileInfo fileInfo);

		/// <summary>
		/// 文件内存解密方法
		/// </summary>
		byte[] LoadFromMemory(DecryptFileInfo fileInfo);

		/// <summary>
		/// 文件流解密方法
		/// </summary>
		System.IO.Stream LoadFromStream(DecryptFileInfo fileInfo);

		/// <summary>
		/// 文件流解密的托管缓存大小
		/// </summary>
		uint GetManagedReadBufferSize();
	}
}