using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using Helper;

namespace LoginClient
{
	class ENetChannel: IMessageChannel
	{
		private readonly Peer peer;

		public ENetChannel(Peer peer)
		{
			this.peer = peer;
		}

		public async void Dispose()
		{
			await this.peer.DisconnectLaterAsync();
			this.peer.Dispose();
		}

		public void SendMessage<T>(ushort opcode, T message, byte channelID = 0)
		{
			byte[] protoBytes = ProtobufHelper.ToBytes(message);
			var neworkBytes = new byte[sizeof(ushort) + protoBytes.Length];

			var opcodeBytes = BitConverter.GetBytes(opcode);
			opcodeBytes.CopyTo(neworkBytes, 0);
			protoBytes.CopyTo(neworkBytes, sizeof(ushort));
			this.peer.WriteAsync(channelID, neworkBytes);
		}

		public async Task<Tuple<ushort, byte[]>> RecvMessage()
		{
			Packet packet = await this.peer.ReadAsync();
			byte[] bytes = packet.Bytes;
			const int opcodeSize = sizeof (ushort);
			ushort opcode = BitConverter.ToUInt16(bytes, 0);
			var messageBytes = new byte[packet.Length - opcodeSize];
			Array.Copy(bytes, opcodeSize, messageBytes, 0, messageBytes.Length);
			return Tuple.Create(opcode, messageBytes);
		}
	}
}
