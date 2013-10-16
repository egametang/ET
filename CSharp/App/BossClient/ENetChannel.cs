using System;
using System.Threading.Tasks;
using BossBase;
using ENet;
using Helper;
using Ionic.Zlib;

namespace BossClient
{
	class ENetChannel: IMessageChannel
	{
		private readonly ESocket eSocket;

		public ENetChannel(ESocket eSocket)
		{
			this.eSocket = eSocket;
		}

		public async void Dispose()
		{
			await this.eSocket.DisconnectLaterAsync();
			this.eSocket.Dispose();
		}

		public void SendMessage<T>(ushort opcode, T message, byte channelID = 0)
		{
			byte[] protoBytes = ProtobufHelper.ToBytes(message);
			var neworkBytes = new byte[sizeof(ushort) + protoBytes.Length];

			var opcodeBytes = BitConverter.GetBytes(opcode);
			opcodeBytes.CopyTo(neworkBytes, 0);
			protoBytes.CopyTo(neworkBytes, sizeof(ushort));
			this.eSocket.WriteAsync(neworkBytes, channelID);
		}

		public async Task<Tuple<ushort, byte[]>> RecvMessage()
		{
			var bytes = await this.eSocket.ReadAsync();
			const int opcodeSize = sizeof(ushort);
			ushort opcode = BitConverter.ToUInt16(bytes, 0);
			byte flag = bytes[2];
			if (flag == 0)
			{
				var messageBytes = new byte[bytes.Length - opcodeSize - 1];
				Array.Copy(bytes, opcodeSize + 1, messageBytes, 0, messageBytes.Length);
				return Tuple.Create(opcode, messageBytes);
			}
			else
			{
				var messageBytes = new byte[bytes.Length - opcodeSize - 5];
				Array.Copy(bytes, opcodeSize + 5, messageBytes, 0, messageBytes.Length);
				messageBytes = ZlibStream.UncompressBuffer(messageBytes);
				return Tuple.Create(opcode, messageBytes);
			}
		}
	}
}
