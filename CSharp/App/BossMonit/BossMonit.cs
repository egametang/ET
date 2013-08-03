
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using ProtoBuf;

// 这个库要用到protobuf-net库,用Nuget安装即可

namespace BossMonit
{
	static class InternalApi
	{
		[DllImport("internal.dll")]
		public static extern ulong hash_string(string data, int size);
	};

	public static class ProtobufHelper
	{
		public static byte[] ToBytes<T>(T message)
		{
			var ms = new MemoryStream();
			Serializer.Serialize(ms, message);
			return ms.ToArray();
		}

		public static T FromBytes<T>(byte[] bytes)
		{
			var ms = new MemoryStream(bytes, 0, bytes.Length);
			return Serializer.Deserialize<T>(ms);
		}

		public static T FromBytes<T>(byte[] bytes, int index, int length)
		{
			var ms = new MemoryStream(bytes, index, length);
			return Serializer.Deserialize<T>(ms);
		}
	}

	public class BossMonit: IDisposable
	{
		private readonly TcpClient tcpClient;

		public BossMonit(string host, ushort port)
		{
			this.tcpClient = new TcpClient();
			this.tcpClient.Connect(host, port);
		}

		public GmResult GmControl(GmRequest request)
		{
			var stream = tcpClient.GetStream();

			// 发送
			// magic_num
			stream.WriteByte(0xAA);

			// call_guid, 同步调用这个字段随意设置
			var bytes = BitConverter.GetBytes((ulong) 0);
			stream.Write(bytes, 0, bytes.Length);

			// method_id
			const string method_full_name = "boss.GmControl";
			ulong method_id = InternalApi.hash_string(method_full_name, method_full_name.Length);
			bytes = BitConverter.GetBytes(method_id);

			var requestBytes = ProtobufHelper.ToBytes(request);

			// size
			bytes = BitConverter.GetBytes(requestBytes.Length);
			stream.Write(bytes, 0, bytes.Length);

			// request content
			stream.Write(requestBytes, 0, requestBytes.Length);

			// 接收
			var recvBuffer = new byte[21];
			stream.Read(recvBuffer, 0, recvBuffer.Length);

			Debug.Assert(recvBuffer[0] == 0xAA);
			uint return_size = BitConverter.ToUInt32(recvBuffer, 1 + 8 + 8);

			recvBuffer = new byte[return_size];
			stream.Read(recvBuffer, 0, recvBuffer.Length);

			var response = ProtobufHelper.FromBytes<GmResult>(recvBuffer);
			return response;
		}

		public void Dispose()
		{
			tcpClient.Close();
		}
	}
}
