using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using Helper;
using Log;

namespace BossClient
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
			using (Packet packet = await this.peer.ReadAsync())
			{
				byte[] bytes = packet.Bytes;
				const int opcodeSize = sizeof(ushort);
				ushort opcode = BitConverter.ToUInt16(bytes, 0);
				const int flagSize = sizeof (ushort);
				ushort flag = BitConverter.ToUInt16(bytes, opcodeSize);
				if (flag != 0)
				{
					Logger.Debug("packet zip");
					throw new BossException("packet zip!");
				}
				var messageBytes = new byte[packet.Length - opcodeSize - flagSize];
				Array.Copy(bytes, opcodeSize + flagSize, messageBytes, 0, messageBytes.Length);
				return Tuple.Create(opcode, messageBytes);
			}
		}
	}
}
