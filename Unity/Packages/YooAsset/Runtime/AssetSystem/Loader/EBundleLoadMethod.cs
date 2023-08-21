
namespace YooAsset
{
	/// <summary>
	/// Bundle文件的加载方法
	/// </summary>
	public enum EBundleLoadMethod
	{
		/// <summary>
		/// 正常加载（不需要解密）
		/// </summary>
		Normal = 0,
		
		/// <summary>
		/// 通过文件偏移来解密加载
		/// </summary>
		LoadFromFileOffset = 1,

		/// <summary>
		/// 通过文件内存来解密加载
		/// </summary>
		LoadFromMemory = 2,
		
		/// <summary>
		/// 通过文件流来解密加载
		/// </summary>
		LoadFromStream = 3,
	}
}