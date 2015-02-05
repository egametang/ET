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
		private const int packetSizeMax = 128 * 1024;
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
				switch (this.state)
				{
					case ParserState.PacketSize:
						if (this.buffer.Count < 4)
						{
							finish = true;
						}
						else
						{
							this.buffer.RecvFrom(this.packetSizeBuffer);
							this.packetSize = BitConverter.ToInt32(this.packetSizeBuffer, 0);
							this.state = ParserState.PacketBody;
						}
						break;
					case ParserState.PacketBody:
						if (this.buffer.Count < this.packetSize)
						{
							finish = true;
						}
						else
						{
							this.packet = new byte[this.packetSize];
							this.buffer.RecvFrom(this.packet);
							this.isOK = true;
							this.state = ParserState.PacketSize;
							finish = true;
						}
						break;
				}
			}
			return this.isOK;
		}

		public byte[] GetPacket()
		{
			byte[] result = this.packet;
			this.isOK = false;
			return result;
		}
	}
}