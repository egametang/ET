using System.Collections;
using System.Collections.Generic;

namespace UniFramework.Network
{
	/// <summary>
	/// 网络包解码器
	/// </summary>
	public interface INetPackageDecoder
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
		/// 网络消息解码
		/// </summary>
		/// <param name="packageBodyMaxSize">包体的最大尺寸</param>
		/// <param name="ringBuffer">解码需要的字节缓冲区</param>
		/// <param name="outputPackages">接收的包裹列表</param>
		void Decode(int packageBodyMaxSize, RingBuffer ringBuffer, List<INetPackage> outputPackages);
	}
}