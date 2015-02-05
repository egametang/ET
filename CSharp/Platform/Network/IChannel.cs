using System;
using System.Threading.Tasks;

namespace Network
{
	[Flags]
	public enum PacketFlags
	{
		None = 0,
		Reliable = 1 << 0,
		Unsequenced = 1 << 1,
		NoAllocate = 1 << 2
	}

	public interface IChannel: IDisposable
	{
		/// <summary>
		/// 发送消息
		/// </summary>
		void SendAsync(byte[] buffer, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable);

		/// <summary>
		/// 接收消息
		/// </summary>
		Task<byte[]> RecvAsync();

		Task<bool> DisconnnectAsync();

		string RemoteAddress { get; }
	}
}