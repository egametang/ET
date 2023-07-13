using System.Collections;
using System.Collections.Generic;

namespace UniFramework.Network
{
	/// <summary>
	/// 网络包编码器
	/// </summary>
	public class DefaultNetPackageEncoder : INetPackageEncoder
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
		/// 编码
		/// </summary>
		/// <param name="packageBodyMaxSize">包体的最大尺寸</param>
		/// <param name="ringBuffer">编码填充的字节缓冲区</param>
		/// <param name="encodePackage">发送的包裹</param>
		public void Encode(int packageBodyMaxSize, RingBuffer ringBuffer, INetPackage encodePackage)
		{
			if (encodePackage == null)
			{
				_handleErrorCallback(false, "The encode package object is null");
				return;
			}

			DefaultNetPackage package = (DefaultNetPackage)encodePackage;
			if (package == null)
			{
				_handleErrorCallback(false, $"The encode package object is invalid : {encodePackage.GetType()}");
				return;
			}

			// 检测逻辑是否合法
			if (package.BodyBytes == null)
			{
				_handleErrorCallback(false, $"The encode package BodyBytes field is null : {encodePackage.GetType()}");
				return;
			}

			// 获取包体数据
			byte[] bodyData = package.BodyBytes;

			// 检测包体长度
			if (bodyData.Length > packageBodyMaxSize)
			{
				_handleErrorCallback(false, $"The encode package {package.MsgID} body size is larger than {packageBodyMaxSize}");
				return;
			}

			// 写入长度
			int packetLength = PackageHeaderIDFiledSize + bodyData.Length;
			ringBuffer.WriteInt(packetLength);

			// 写入包头
			{
				// 写入消息ID
				ringBuffer.WriteInt(package.MsgID);
			}

			// 写入包体
			ringBuffer.WriteBytes(bodyData, 0, bodyData.Length);
		}
	}
}