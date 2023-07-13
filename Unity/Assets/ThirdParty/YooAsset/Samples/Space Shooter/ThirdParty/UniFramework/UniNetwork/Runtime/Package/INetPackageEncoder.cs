using System.Collections;
using System.Collections.Generic;

namespace UniFramework.Network
{
	/// <summary>
	/// 网络包编码器
	/// </summary>
	public interface INetPackageEncoder
	{
		/// <summary>
		/// 获取包头的尺寸
		/// </summary>
		int GetPackageHeaderSize();

		/// <summary>
		/// 注册异常错误回调方法
		/// </summary>
		/// <param name="callback"></param>
		void RigistHandleErrorCallback(HandleErrorDelegate callback);

		/// <summary>
		/// 编码
		/// </summary>
		/// <param name="packageBodyMaxSize">包体的最大尺寸</param>
		/// <param name="ringBuffer">编码填充的字节缓冲区</param>
		/// <param name="encodePackage">发送的包裹</param>
		void Encode(int packageBodyMaxSize, RingBuffer ringBuffer, INetPackage encodePackage);
	}
}