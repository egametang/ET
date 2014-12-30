using System;

namespace TNet
{
	internal enum ParserState
	{
		PacketSize,
		PacketBody,
	}

	internal class PacketParser
	{
		private readonly TBuffer buffer;

		private int packetSize;
		private readonly byte[] packetSizeBuffer = new byte[4];
		private ParserState state;
		private byte[] packet;
		private bool isOK;

		public PacketParser(TBuffer buffer)
		{
			this.buffer = buffer;
		}

		public bool Parse()
		{
			if (this.isOK)
			{
				return true;
			}

			bool finish = false;
			while (!finish)
			{
				switch (state)
				{
					case ParserState.PacketSize:
						if (buffer.Count < 4)
						{
							finish = true;
						}
						else
						{
							buffer.RecvFrom(packetSizeBuffer);
							packetSize = BitConverter.ToInt32(packetSizeBuffer, 0);
							state = ParserState.PacketBody;
						}
						break;
					case ParserState.PacketBody:
						if (buffer.Count < packetSize)
						{
							finish = true;
						}
						else
						{
							this.packet = new byte[packetSize];
							buffer.RecvFrom(this.packet);
							this.isOK = true;
							state = ParserState.PacketSize;
							finish = true;
						}
						break;
				}
			}
			return this.isOK;
		}

		public byte[] GetPacket()
		{
			byte[] result = packet;
			this.isOK = false;
			return result;
		}
	}
}
