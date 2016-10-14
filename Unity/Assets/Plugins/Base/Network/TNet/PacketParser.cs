using System;

namespace Base
{
	internal enum ParserState
	{
		PacketSize,
		PacketBody
	}

	internal class PacketParser
	{
		private readonly TBuffer buffer;

		private uint packetSize;
		private readonly byte[] packetSizeBuffer = new byte[4];
		private ParserState state;
		private byte[] packet;
		private bool isOK;

		public PacketParser(TBuffer buffer)
		{
			this.buffer = buffer;
		}

		private bool Parse()
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
							this.packetSize = BitConverter.ToUInt32(this.packetSizeBuffer, 0);
							if (packetSize > 1024 * 1024)
							{
								throw new Exception($"packet too large, size: {this.packetSize}");
							}
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
			this.Parse();
			if (!this.isOK)
			{
				return null;
			}
			byte[] result = this.packet;
			this.isOK = false;
			return result;
		}
	}
}