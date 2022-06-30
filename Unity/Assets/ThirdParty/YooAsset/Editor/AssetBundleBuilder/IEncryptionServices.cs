
namespace YooAsset.Editor
{
	public interface IEncryptionServices
	{
		/// <summary>
		/// 检测是否需要加密
		/// </summary>
		bool Check(string bundleName);
		
		/// <summary>
		/// 加密方法
		/// </summary>
		/// <param name="fileData">要加密的文件数据</param>
		/// <returns>返回加密后的字节数据</returns>
		byte[] Encrypt(byte[] fileData);
	}
}