using System.Collections;
using System.Collections.Generic;

namespace UniFramework.Network
{
	/// <summary>
	/// 网络包解码器
	/// </summary>
	public class DefaultNetPackageDecoder : INetPackageDecoder
	{
		private HandleErrorDelegate _handleErrorCallback;
		private const int PackageHeaderLengthFiledSize = 4; //int类型
		private const int PackageHeaderIDFiledSize = 4; //int类型

		/// <summary>
		/// 获取包头的尺寸
		/// </summary>
		public int GetPackageHeaderSize()
		{
			return PackageHeaderLengthFiledSize + PackageHeaderIDFiledSize;
		}

		/// <summary>
		/// 注册异常错误回调方法
		/// </summary>
		/// <param name="callback"></param>
		public void RigistHandleErrorCallback(HandleErrorDelegate callback)
		{
			_handleErrorCallback = callback;
		}

		/// <summary>
		/// 网络消息解码
		/// </summary>
		/// <param name="packageBodyMaxSize">包体的最大尺寸</param>
		/// <param name="ringBuffer">解码需要的字节缓冲区</param>
		/// <param name="outputPackages">接收的包裹列表</param>
		public void Decode(int packageBodyMaxSize, RingBuffer ringBuffer, List<INetPackage> outputPackages)
		{
			// 循环解包
			while (true)
			{
				// 如果数据不够判断消息长度
				if (ringBuffer.ReadableBytes < PackageHeaderLengthFiledSize)
					break;

				ringBuffer.MarkReaderIndex();

				// 读取Package长度
				int packageSize = ringBuffer.ReadInt();

				// 如果剩余可读数据小于Package长度
				if (ringBuffer.ReadableBytes < packageSize)
				{
					ringBuffer.ResetReaderIndex();
					break; //需要退出读够数据再解包
				}

				DefaultNetPackage package = new DefaultNetPackage();

				// 读取包头
				{
					// 读取消息ID
					package.MsgID = ringBuffer.ReadInt();
				}

				// 检测包体长度
				int bodySize = packageSize - PackageHeaderIDFiledSize;
				if (bodySize > packageBodyMaxSize)
				{
					_handleErrorCallback(true, $"The decode package {package.MsgID} body size is larger than {packageBodyMaxSize} !");
					break;
				}

				// 读取包体
				{
					package.BodyBytes = ringBuffer.ReadBytes(bodySize);
					outputPackages.Add(package);
				}
			}

			// 注意：将剩余数据移至起始
			ringBuffer.DiscardReadBytes();
		}
	}
}