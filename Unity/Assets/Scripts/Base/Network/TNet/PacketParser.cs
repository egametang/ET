using System;

namespace Model
{
	internal enum ParserState
	{
		PacketSize,
		PacketBody
	}

	internal class PacketParser
	{
		private readonly TBuffer buffer;

		private ushort packetSize;
		private readonly byte[] packetSizeBuffer = new byte[2];
		private ParserState state;
		private byte[] packet;
		private bool isOK;

		public PacketParser(TBuffer buffer)
		{
			this.buffer = buffer;
		}

		private void Parse()
		{
			if (this.isOK)
			{
				return;
			}

			bool finish = false;
			while (!finish)
			{
				switch (this.state)
				{
					case ParserState.PacketSize:
						if (this.buffer.Count < 2)
						{
							finish = true;
						}
						else
						{
							this.buffer.RecvFrom(this.packetSizeBuffer);
							this.packetSize = BitConverter.ToUInt16(this.packetSizeBuffer, 0);
							this.packetSize = NetworkHelper.NetworkToHostOrder(this.packetSize);
							if (packetSize > 60000)
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