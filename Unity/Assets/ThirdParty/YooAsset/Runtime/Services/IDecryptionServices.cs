
namespace YooAsset
{
	public interface IDecryptionServices
	{
		/// <summary>
		/// 获取加密文件的数据偏移量
		/// </summary>
		ulong GetFileOffset();
	}
}